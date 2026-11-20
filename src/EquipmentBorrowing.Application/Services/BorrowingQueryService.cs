using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowingQueryService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public BorrowingQueryService(
        IBorrowingRepository borrowingRepository,
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<OperationResult<IEnumerable<Borrowing>>> GetActiveBorrowingsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _borrowingRepository.GetActiveAsync(cancellationToken);
            return OperationResult<IEnumerable<Borrowing>>.Success(items);
        }
        catch (Exception)
        {
            return OperationResult<IEnumerable<Borrowing>>.Failure("Unable to load active borrowings.");
        }
    }

    public async Task<OperationResult<IEnumerable<Borrowing>>> GetStudentHistoryAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
                return OperationResult<IEnumerable<Borrowing>>.ValidationFailed("Selected student was not found.");

            var history = await _borrowingRepository.GetByStudentIdAsync(studentId, cancellationToken);
            return OperationResult<IEnumerable<Borrowing>>.Success(history);
        }
        catch (Exception)
        {
            return OperationResult<IEnumerable<Borrowing>>.Failure("Unable to load student borrowing history.");
        }
    }

    public async Task<OperationResult<IEnumerable<Borrowing>>> GetEquipmentHistoryAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
            if (equipment is null)
                return OperationResult<IEnumerable<Borrowing>>.ValidationFailed("Selected equipment was not found.");

            var history = await _borrowingRepository.GetByEquipmentIdAsync(equipmentId, cancellationToken);
            return OperationResult<IEnumerable<Borrowing>>.Success(history);
        }
        catch (Exception)
        {
            return OperationResult<IEnumerable<Borrowing>>.Failure("Unable to load equipment borrowing history.");
        }
    }

    public async Task<OperationResult<IEnumerable<Borrowing>>> SearchBorrowingsAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        BorrowingSortField sortField,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await _borrowingRepository.SearchAsync(
                searchText,
                statusFilter,
                sortField,
                sortDirection,
                cancellationToken);

            return OperationResult<IEnumerable<Borrowing>>.Success(results);
        }
        catch (Exception)
        {
            return OperationResult<IEnumerable<Borrowing>>.Failure("Unable to search or filter borrowings.");
        }
    }
}