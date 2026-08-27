using System;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<bool> ExecuteAsync(int studentId, int equipmentId, int days)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null || !student.IsAllowedToBorrow) return false; // Exists & Allowed

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
        if (equipment == null || !equipment.IsAvailable) return false; // Exists & Available

        var activeCount = await _borrowingRepository.GetActiveCountByStudentIdAsync(studentId);
        if (activeCount >= 3) return false; // Max limit check

        equipment.IsAvailable = false;
        await _equipmentRepository.UpdateAsync(equipment);

        // Date calculations are now handled automatically by the constructor
        var borrowing = new Borrowing(new Random().Next(1, 1000), studentId, equipmentId, days);
        await _borrowingRepository.AddAsync(borrowing);

        return true;
    }
}