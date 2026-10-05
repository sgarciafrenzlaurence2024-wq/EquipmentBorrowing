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

    public async Task<IEnumerable<Borrowing>> GetAllAsync()
    {
        await using var context = _factory.CreateDbContext();
        return await context.Borrowings.AsNoTracking().ToListAsync();
    }

    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = _factory.CreateDbContext();
        return await context.Borrowings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await using var context = _factory.CreateDbContext();
        await context.Borrowings.AddAsync(borrowing, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await using var context = _factory.CreateDbContext();
        context.Borrowings.Update(borrowing);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetActiveCountByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = _factory.CreateDbContext();
        return await context.Borrowings
            .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);
    }
}   