using System;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private const int MaxActiveBorrowings = 3;

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
        if (days <= 0)
            return false;

        var now = DateTime.UtcNow;
        var result = await BorrowAsync(studentId, equipmentId, now, now.AddDays(days));
        return result.IsSuccess;
    }

    public async Task<OperationResult<Borrowing>> BorrowAsync(
        int studentId,
        int equipmentId,
        DateTime checkoutDate,
        DateTime expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        var utcCheckoutDate = NormalizeToUtc(checkoutDate);
        var utcExpectedReturnDate = NormalizeToUtc(expectedReturnDate);

        if (utcExpectedReturnDate <= utcCheckoutDate)
            return OperationResult<Borrowing>.ValidationFailed(
                "Expected return date must be later than the checkout date.");

        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
                return OperationResult<Borrowing>.ValidationFailed("Selected student does not exist.");

            if (!student.IsAllowedToBorrow)
                return OperationResult<Borrowing>.ValidationFailed(
                    "Selected student is currently restricted from borrowing equipment.");

            var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
            if (equipment is null)
                return OperationResult<Borrowing>.ValidationFailed("Selected equipment does not exist.");

            if (!equipment.IsAvailable)
                return OperationResult<Borrowing>.ValidationFailed(
                    "Selected equipment is currently unavailable.");

            var activeCount = await _borrowingRepository.GetActiveCountByStudentIdAsync(studentId, cancellationToken);
            if (activeCount >= MaxActiveBorrowings)
                return OperationResult<Borrowing>.ValidationFailed(
                    $"Student has already reached the maximum limit of {MaxActiveBorrowings} active borrowings.");

            equipment.MarkAsBorrowed();
            await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

            try
            {
                var borrowing = new Borrowing(studentId, equipmentId, utcCheckoutDate, utcExpectedReturnDate);
                await _borrowingRepository.AddAsync(borrowing, cancellationToken);

                return OperationResult<Borrowing>.Success(borrowing, "Equipment borrowed successfully.");
            }
            catch
            {
                equipment.MarkAsReturned();
                await _equipmentRepository.UpdateAsync(equipment, CancellationToken.None);
                throw;
            }
        }
        catch (Exception)
        {
            return OperationResult<Borrowing>.Failure(
                "Unable to complete borrowing due to a persistence error. Please refresh and try again.");
        }
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}