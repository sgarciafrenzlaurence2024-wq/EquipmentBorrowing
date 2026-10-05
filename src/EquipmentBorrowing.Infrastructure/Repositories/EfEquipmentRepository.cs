using System.Collections.Generic;
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

    public async Task<IEnumerable<Equipment>> GetAllAsync()
    {
        await using var context = _factory.CreateDbContext();
        return await context.Equipment.AsNoTracking().ToListAsync();
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = _factory.CreateDbContext();
        return await context.Equipment.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await using var context = _factory.CreateDbContext();
        context.Equipment.Update(equipment);
        await context.SaveChangesAsync(cancellationToken);
    }
}