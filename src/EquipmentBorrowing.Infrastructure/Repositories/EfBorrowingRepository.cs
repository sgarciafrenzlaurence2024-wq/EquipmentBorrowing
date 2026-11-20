using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingRepository : IBorrowingRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;

    public EfBorrowingRepository(IDbContextFactory<EquipmentBorrowingDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Borrowing>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .OrderByDescending(b => b.BorrowedDate)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .Where(b => b.Status == BorrowingStatus.Active)
            .OrderByDescending(b => b.BorrowedDate)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<int> GetActiveCountByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .AsNoTracking()
            .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);
    }

    public async Task<IEnumerable<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.BorrowedDate)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .Where(b => b.EquipmentId == equipmentId)
            .OrderByDescending(b => b.BorrowedDate)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        BorrowingSortField sortField,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        var query = context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(b => b.Status == statusFilter.Value);
        }

        var trimmedSearch = searchText?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedSearch))
        {
            var pattern = $"%{trimmedSearch}%";
            query = query.Where(b =>
                (b.Student != null && (
                    EF.Functions.ILike(b.Student.Name, pattern) ||
                    EF.Functions.ILike(b.Student.StudentNumber, pattern))) ||
                (b.Equipment != null && EF.Functions.ILike(b.Equipment.Name, pattern)));
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

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await context.Borrowings.AddAsync(borrowing, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        context.Borrowings.Update(borrowing);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddMultipleAtomicAsync(
        IEnumerable<Borrowing> borrowings,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var borrowing in borrowings)
        {
            await context.Borrowings.AddAsync(borrowing, cancellationToken);
            var equipment = await context.Equipment.FirstOrDefaultAsync(e => e.Id == borrowing.EquipmentId, cancellationToken);
            if (equipment is not null)
            {
                equipment.MarkAsBorrowed();
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}