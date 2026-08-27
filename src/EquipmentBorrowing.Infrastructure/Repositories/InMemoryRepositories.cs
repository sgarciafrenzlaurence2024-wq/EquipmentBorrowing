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

    // --- IStudentRepository Implementation ---
    Task<Student?> IStudentRepository.GetByIdAsync(int id, CancellationToken ct)
    {
        return Task.FromResult(Students.FirstOrDefault(s => s.Id == id));
    }

    // --- IEquipmentRepository Implementation ---
    Task<Equipment?> IEquipmentRepository.GetByIdAsync(int id, CancellationToken ct)
    {
        return Task.FromResult(Equipments.FirstOrDefault(e => e.Id == id));
    }

    Task IEquipmentRepository.UpdateAsync(Equipment equipment, CancellationToken ct)
    {
        var existing = Equipments.FirstOrDefault(e => e.Id == equipment.Id);
        if (existing != null) existing.IsAvailable = equipment.IsAvailable;
        return Task.CompletedTask;
    }

    // --- IBorrowingRepository Implementation ---
    Task<int> IBorrowingRepository.GetActiveCountByStudentIdAsync(int studentId, CancellationToken ct)
    {
        return Task.FromResult(Borrowings.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active));
    }

    Task<Borrowing?> IBorrowingRepository.GetByIdAsync(int id, CancellationToken ct)
    {
        return Task.FromResult(Borrowings.FirstOrDefault(b => b.Id == id));
    }

    Task IBorrowingRepository.AddAsync(Borrowing borrowing, CancellationToken ct)
    {
        Borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    Task IBorrowingRepository.UpdateAsync(Borrowing borrowing, CancellationToken ct)
    {
        var existing = Borrowings.FirstOrDefault(b => b.Id == borrowing.Id);
        if (existing != null) existing.Status = borrowing.Status;
        return Task.CompletedTask;
    }
}