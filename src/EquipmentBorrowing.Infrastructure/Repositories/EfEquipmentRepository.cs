using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;

    public EfEquipmentRepository(IDbContextFactory<EquipmentBorrowingDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .Where(e => e.IsAvailable && !context.Borrowings.Any(b => b.EquipmentId == e.Id && b.Status == BorrowingStatus.Active))
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNormalizedCodeAsync(
        string normalizedCode,
        int? excludeEquipmentId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .AnyAsync(e =>
                e.NormalizedCode == normalizedCode &&
                (!excludeEquipmentId.HasValue || e.Id != excludeEquipmentId.Value),
                cancellationToken);
    }

    public async Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await context.Equipment.AddAsync(equipment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        context.Equipment.Update(equipment);
        await context.SaveChangesAsync(cancellationToken);
    }
}