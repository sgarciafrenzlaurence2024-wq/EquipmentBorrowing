using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class DatabaseResetService : IDatabaseResetService
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _contextFactory;

    public DatabaseResetService(IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<OperationResult> ResetAndSeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            context.Borrowings.RemoveRange(context.Borrowings);
            await context.SaveChangesAsync(cancellationToken);

            context.Equipment.RemoveRange(context.Equipment);
            context.Students.RemoveRange(context.Students);
            await context.SaveChangesAsync(cancellationToken);

            context.ChangeTracker.Clear();
            DatabaseSeeder.Seed(context);
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return OperationResult.Success("Database reset and re-seeded successfully.");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.UnknownOutcome(
                "Reset outcome is uncertain because the operation was interrupted. Refresh state before retrying.");
        }
        catch (Exception)
        {
            return OperationResult.Failure("Failed to reset database. Please verify connection and try again.");
        }
    }
}