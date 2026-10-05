using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfStudentRepository : IStudentRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;

    public EfStudentRepository(IDbContextFactory<EquipmentBorrowingDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        await using var context = _factory.CreateDbContext();
        return await context.Students.AsNoTracking().ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = _factory.CreateDbContext();
        return await context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        await using var context = _factory.CreateDbContext();
        context.Students.Update(student);
        await context.SaveChangesAsync(cancellationToken);
    }
}