using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryRepositories : IStudentRepository, IEquipmentRepository, IBorrowingRepository
{
    public List<Student> Students { get; } = new();
    public List<Equipment> Equipments { get; } = new();
    public List<Borrowing> Borrowings { get; } = new();

    private int _nextStudentId = 1;
    private int _nextEquipmentId = 1;
    private int _nextBorrowingId = 1;

    public InMemoryRepositories()
    {
        AddSeedEquipment(new Equipment("EQ-0001", "Android Smartphone (NoteGuard Test Device)", "Test device", isAvailable: true));
        AddSeedEquipment(new Equipment("EQ-0002", "Cisco Catalyst 2960 Switch", "Network switch", isAvailable: true));
        AddSeedEquipment(new Equipment("EQ-0003", "Systems Analysis Toolkit", "Analysis kit", isAvailable: true));

        AddSeedStudent(new Student("STU-0001", "Frenz Laurence", "frenz@campus.edu", isAllowedToBorrow: true));
        AddSeedStudent(new Student("STU-0002", "Katyy Dyy", "katyy@campus.edu", isAllowedToBorrow: true));
        AddSeedStudent(new Student("STU-0003", "Kirzten Sunshine", null, isAllowedToBorrow: false));
    }

    private void AddSeedStudent(Student student)
    {
        typeof(Student).GetProperty(nameof(Student.Id))?.SetValue(student, _nextStudentId++);
        Students.Add(student);
    }

    private void AddSeedEquipment(Equipment equipment)
    {
        typeof(Equipment).GetProperty(nameof(Equipment.Id))?.SetValue(equipment, _nextEquipmentId++);
        Equipments.Add(equipment);
    }

    private void AttachNavigations(Borrowing borrowing)
    {
        borrowing.Student ??= Students.FirstOrDefault(s => s.Id == borrowing.StudentId);
        borrowing.Equipment ??= Equipments.FirstOrDefault(e => e.Id == borrowing.EquipmentId);
    }

    // --- IStudentRepository Implementation ---
    Task<IEnumerable<Student>> IStudentRepository.GetAllAsync(CancellationToken ct)
        => Task.FromResult<IEnumerable<Student>>(Students.OrderBy(s => s.Id).ToList());

    Task<Student?> IStudentRepository.GetByIdAsync(int id, CancellationToken ct)
        => Task.FromResult(Students.FirstOrDefault(s => s.Id == id));

    Task<bool> IStudentRepository.ExistsByNormalizedStudentNumberAsync(
        string normalizedStudentNumber,
        int? excludeStudentId,
        CancellationToken ct)
    {
        var exists = Students.Any(s =>
            s.NormalizedStudentNumber == normalizedStudentNumber &&
            (!excludeStudentId.HasValue || s.Id != excludeStudentId.Value));
        return Task.FromResult(exists);
    }

    Task IStudentRepository.AddAsync(Student student, CancellationToken ct)
    {
        if (student.Id == 0)
        {
            typeof(Student).GetProperty(nameof(Student.Id))?.SetValue(student, _nextStudentId++);
        }
        Students.Add(student);
        return Task.CompletedTask;
    }

    Task IStudentRepository.UpdateAsync(Student student, CancellationToken ct)
    {
        var idx = Students.FindIndex(s => s.Id == student.Id);
        if (idx >= 0)
        {
            Students[idx] = student;
        }
        return Task.CompletedTask;
    }

    // --- IEquipmentRepository Implementation ---
    Task<IEnumerable<Equipment>> IEquipmentRepository.GetAllAsync(CancellationToken ct)
        => Task.FromResult<IEnumerable<Equipment>>(Equipments.OrderBy(e => e.Id).ToList());

    Task<IEnumerable<Equipment>> IEquipmentRepository.GetAvailableAsync(CancellationToken ct)
    {
        var available = Equipments
            .Where(e => e.IsAvailable && !Borrowings.Any(b => b.EquipmentId == e.Id && b.Status == BorrowingStatus.Active))
            .OrderBy(e => e.Id)
            .ToList();
        return Task.FromResult<IEnumerable<Equipment>>(available);
    }

    Task<Equipment?> IEquipmentRepository.GetByIdAsync(int id, CancellationToken ct)
        => Task.FromResult(Equipments.FirstOrDefault(e => e.Id == id));

    Task<bool> IEquipmentRepository.ExistsByNormalizedCodeAsync(
        string normalizedCode,
        int? excludeEquipmentId,
        CancellationToken ct)
    {
        var exists = Equipments.Any(e =>
            e.NormalizedCode == normalizedCode &&
            (!excludeEquipmentId.HasValue || e.Id != excludeEquipmentId.Value));
        return Task.FromResult(exists);
    }

    Task IEquipmentRepository.AddAsync(Equipment equipment, CancellationToken ct)
    {
        if (equipment.Id == 0)
        {
            typeof(Equipment).GetProperty(nameof(Equipment.Id))?.SetValue(equipment, _nextEquipmentId++);
        }
        Equipments.Add(equipment);
        return Task.CompletedTask;
    }

    Task IEquipmentRepository.UpdateAsync(Equipment equipment, CancellationToken ct)
    {
        var idx = Equipments.FindIndex(e => e.Id == equipment.Id);
        if (idx >= 0)
        {
            Equipments[idx] = equipment;
        }
        return Task.CompletedTask;
    }

    // --- IBorrowingRepository Implementation ---
    Task<IEnumerable<Borrowing>> IBorrowingRepository.GetAllAsync(CancellationToken ct)
    {
        foreach (var b in Borrowings) AttachNavigations(b);
        return Task.FromResult<IEnumerable<Borrowing>>(
            Borrowings.OrderByDescending(b => b.BorrowedDate).ThenBy(b => b.Id).ToList());
    }

    Task<IEnumerable<Borrowing>> IBorrowingRepository.GetActiveAsync(CancellationToken ct)
    {
        foreach (var b in Borrowings) AttachNavigations(b);
        return Task.FromResult<IEnumerable<Borrowing>>(
            Borrowings.Where(b => b.Status == BorrowingStatus.Active)
                      .OrderByDescending(b => b.BorrowedDate).ThenBy(b => b.Id).ToList());
    }

    Task<Borrowing?> IBorrowingRepository.GetByIdAsync(int id, CancellationToken ct)
    {
        var borrowing = Borrowings.FirstOrDefault(b => b.Id == id);
        if (borrowing is not null) AttachNavigations(borrowing);
        return Task.FromResult(borrowing);
    }

    Task<int> IBorrowingRepository.GetActiveCountByStudentIdAsync(int studentId, CancellationToken ct)
        => Task.FromResult(Borrowings.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active));

    Task<IEnumerable<Borrowing>> IBorrowingRepository.GetByStudentIdAsync(int studentId, CancellationToken ct)
    {
        foreach (var b in Borrowings) AttachNavigations(b);
        return Task.FromResult<IEnumerable<Borrowing>>(
            Borrowings.Where(b => b.StudentId == studentId)
                      .OrderByDescending(b => b.BorrowedDate).ThenBy(b => b.Id).ToList());
    }

    Task<IEnumerable<Borrowing>> IBorrowingRepository.GetByEquipmentIdAsync(int equipmentId, CancellationToken ct)
    {
        foreach (var b in Borrowings) AttachNavigations(b);
        return Task.FromResult<IEnumerable<Borrowing>>(
            Borrowings.Where(b => b.EquipmentId == equipmentId)
                      .OrderByDescending(b => b.BorrowedDate).ThenBy(b => b.Id).ToList());
    }

    Task<IEnumerable<Borrowing>> IBorrowingRepository.SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        BorrowingSortField sortField,
        SortDirection sortDirection,
        CancellationToken ct)
    {
        foreach (var b in Borrowings) AttachNavigations(b);

        var query = Borrowings.AsEnumerable();

        if (statusFilter.HasValue)
            query = query.Where(b => b.Status == statusFilter.Value);

        var term = searchText?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(b =>
                (b.Student != null && (
                    b.Student.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    b.Student.StudentNumber.Contains(term, StringComparison.OrdinalIgnoreCase))) ||
                (b.Equipment != null &&
                    b.Equipment.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        query = (sortField, sortDirection) switch
        {
            (BorrowingSortField.ExpectedReturnDate, SortDirection.Ascending) =>
                query.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, SortDirection.Descending) =>
                query.OrderByDescending(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            (BorrowingSortField.BorrowedDate, SortDirection.Ascending) =>
                query.OrderBy(b => b.BorrowedDate).ThenBy(b => b.Id),
            _ =>
                query.OrderByDescending(b => b.BorrowedDate).ThenBy(b => b.Id)
        };

        return Task.FromResult<IEnumerable<Borrowing>>(query.ToList());
    }

    Task IBorrowingRepository.AddAsync(Borrowing borrowing, CancellationToken ct)
    {
        if (borrowing.Id == 0)
        {
            typeof(Borrowing).GetProperty(nameof(Borrowing.Id))?.SetValue(borrowing, _nextBorrowingId++);
        }
        AttachNavigations(borrowing);
        Borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    Task IBorrowingRepository.UpdateAsync(Borrowing borrowing, CancellationToken ct)
    {
        AttachNavigations(borrowing);
        var idx = Borrowings.FindIndex(b => b.Id == borrowing.Id);
        if (idx >= 0)
        {
            Borrowings[idx] = borrowing;
        }
        return Task.CompletedTask;
    }

    Task IBorrowingRepository.AddMultipleAtomicAsync(IEnumerable<Borrowing> borrowings, CancellationToken ct)
    {
        var list = borrowings.ToList();
        foreach (var b in list)
        {
            if (b.Id == 0)
            {
                typeof(Borrowing).GetProperty(nameof(Borrowing.Id))?.SetValue(b, _nextBorrowingId++);
            }
            var eq = Equipments.FirstOrDefault(e => e.Id == b.EquipmentId);
            if (eq is not null)
            {
                eq.MarkAsBorrowed();
            }
            AttachNavigations(b);
            Borrowings.Add(b);
        }
        return Task.CompletedTask;
    }
}