using System;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private readonly IStudentRepository _studentRepo;
    private readonly IEquipmentRepository _equipmentRepo;
    private readonly IBorrowingRepository _borrowingRepo;
    private const int MaxAllowed = 3;

    // Part F: Receiving dependencies via constructor injection
    public BorrowEquipmentService(
        IStudentRepository studentRepo,
        IEquipmentRepository equipmentRepo,
        IBorrowingRepository borrowingRepo)
    {
        _studentRepo = studentRepo;
        _equipmentRepo = equipmentRepo;
        _borrowingRepo = borrowingRepo;
    }

    public async Task<bool> ExecuteAsync(int studentId, int equipmentId, int durationDays)
    {
        var student = await _studentRepo.GetByIdAsync(studentId);
        if (student == null || !student.IsAllowedToBorrow) return false;

        var equipment = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (equipment == null || !equipment.IsAvailable) return false;

        int activeCount = await _borrowingRepo.GetActiveCountByStudentIdAsync(studentId);
        if (activeCount >= MaxAllowed) return false;

        equipment.MarkAsBorrowed();
        await _equipmentRepo.UpdateAsync(equipment);

        var record = new Borrowing(
            Random.Shared.Next(1, 1000),
            studentId,
            equipmentId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(durationDays)
        );

        await _borrowingRepo.AddAsync(record);
        return true;
    }
}