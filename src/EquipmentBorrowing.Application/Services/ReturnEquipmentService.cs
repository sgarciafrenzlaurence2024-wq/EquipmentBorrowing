using System;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<bool> ExecuteAsync(int borrowingId)
    {
        var result = await ReturnAsync(borrowingId, DateTime.UtcNow);
        return result.IsSuccess;
    }

    public async Task<OperationResult<Borrowing>> ReturnAsync(
        int borrowingId,
        DateTime actualReturnDate,
        CancellationToken cancellationToken = default)
    {
        var utcActualReturnDate = NormalizeToUtc(actualReturnDate);

        try
        {
            var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId, cancellationToken);
            if (borrowing is null)
                return OperationResult<Borrowing>.ValidationFailed("Selected borrowing record was not found.");

            if (borrowing.Status != BorrowingStatus.Active)
                return OperationResult<Borrowing>.ValidationFailed("Borrowing has already been returned.");

            if (utcActualReturnDate < borrowing.BorrowedDate)
                return OperationResult<Borrowing>.ValidationFailed(
                    "Actual return date cannot precede the checkout date.");

            var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);
            if (equipment is null)
                return OperationResult<Borrowing>.ValidationFailed("Associated equipment record was not found.");

            borrowing.MarkReturned(utcActualReturnDate);
            equipment.MarkAsReturned();

            await _borrowingRepository.UpdateAsync(borrowing, cancellationToken);
            await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

            return OperationResult<Borrowing>.Success(borrowing, "Equipment returned successfully.");
        }
        catch (Exception)
        {
            return OperationResult<Borrowing>.Failure(
                "Unable to record return due to a persistence error. Please refresh and try again.");
        }
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}