using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class MultiItemCheckoutService
{
    private const int MaxActiveBorrowings = 3;

    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public MultiItemCheckoutService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<OperationResult<IReadOnlyList<Borrowing>>> CheckoutMultipleAsync(
        int studentId,
        IReadOnlyList<int> equipmentIds,
        DateTime checkoutDate,
        DateTime expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        var utcCheckoutDate = NormalizeToUtc(checkoutDate);
        var utcExpectedReturnDate = NormalizeToUtc(expectedReturnDate);

        if (equipmentIds is null || equipmentIds.Count < 2)
            return OperationResult<IReadOnlyList<Borrowing>>.ValidationFailed(
                "Multi-item checkout requires at least two equipment items.");

        if (equipmentIds.Distinct().Count() != equipmentIds.Count)
            return OperationResult<IReadOnlyList<Borrowing>>.ValidationFailed(
                "Duplicate equipment selections are not allowed in a multi-item checkout.");

        if (utcExpectedReturnDate <= utcCheckoutDate)
            return OperationResult<IReadOnlyList<Borrowing>>.ValidationFailed(
                "Expected return date must be later than the checkout date.");

        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
                return OperationResult<IReadOnlyList<Borrowing>>.ValidationFailed("Selected student does not exist.");

            if (!student.IsAllowedToBorrow)
                return OperationResult<IReadOnlyList<Borrowing>>.ValidationFailed(
                    "Selected student is currently restricted from borrowing equipment.");

            var currentActiveCount = await _borrowingRepository.GetActiveCountByStudentIdAsync(studentId, cancellationToken);
            if (currentActiveCount + equipmentIds.Count > MaxActiveBorrowings)
                return OperationResult<IReadOnlyList<Borrowing>>.ValidationFailed(
                    $"Request exceeds the maximum limit of {MaxActiveBorrowings} active borrowings ({currentActiveCount} active + {equipmentIds.Count} requested).");

            var borrowingsToCreate = new List<Borrowing>(equipmentIds.Count);

            foreach (var equipmentId in equipmentIds)
            {
                var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
                if (equipment is null)
                    return OperationResult<IReadOnlyList<Borrowing>>.ValidationFailed(
                        $"Equipment ID {equipmentId} does not exist.");

                if (!equipment.IsAvailable)
                    return OperationResult<IReadOnlyList<Borrowing>>.ValidationFailed(
                        $"Equipment '{equipment.Name}' is currently unavailable.");

                borrowingsToCreate.Add(new Borrowing(studentId, equipmentId, utcCheckoutDate, utcExpectedReturnDate));
            }

            await _borrowingRepository.AddMultipleAtomicAsync(borrowingsToCreate, cancellationToken);
            return OperationResult<IReadOnlyList<Borrowing>>.Success(
                borrowingsToCreate,
                $"Successfully checked out {borrowingsToCreate.Count} items.");
        }
        catch (OperationCanceledException)
        {
            return OperationResult<IReadOnlyList<Borrowing>>.UnknownOutcome(
                "Checkout outcome is uncertain because the operation was interrupted. Refresh state before retrying.");
        }
        catch (Exception)
        {
            return OperationResult<IReadOnlyList<Borrowing>>.Failure(
                "Multi-item checkout failed and no changes were committed. Please verify state and retry.");
        }
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}