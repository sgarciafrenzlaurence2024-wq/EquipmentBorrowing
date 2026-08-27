using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(IBorrowingRepository borrowingRepo, IEquipmentRepository equipmentRepo)
    {
        _borrowingRepository = borrowingRepo;
        _equipmentRepository = equipmentRepo;
    }

    public async Task<bool> ExecuteAsync(int borrowingId)
    {
        var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (borrowing == null || borrowing.Status != BorrowingStatus.Active) return false;

        var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId);
        if (equipment == null) return false;

        // Mark as returned and free up the equipment
        borrowing.Status = BorrowingStatus.Returned;
        equipment.IsAvailable = true;

        await _borrowingRepository.UpdateAsync(borrowing);
        await _equipmentRepository.UpdateAsync(equipment);

        return true;
    }
}