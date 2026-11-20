using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public enum BorrowingSortField
{
    BorrowedDate,
    ExpectedReturnDate
}

public enum SortDirection
{
    Ascending,
    Descending
}

public interface IBorrowingRepository
{
    Task<IEnumerable<Borrowing>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GetActiveCountByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        BorrowingSortField sortField,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default);
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task AddMultipleAtomicAsync(
        IEnumerable<Borrowing> borrowings,
        CancellationToken cancellationToken = default);
}