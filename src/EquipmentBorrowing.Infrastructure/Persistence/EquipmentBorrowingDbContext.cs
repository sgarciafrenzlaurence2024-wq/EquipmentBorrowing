using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Domain;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();

    public EquipmentBorrowingDbContext()
    {
    }

    public EquipmentBorrowingDbContext(DbContextOptions<EquipmentBorrowingDbContext> options)
        : base(options)
    {
    }


    // Resets the database file and re-seeds default data
    public async Task ResetDatabaseAsync()
    {
        try
        {
            // 1. Clear child table first to satisfy foreign key constraints
            Borrowings.RemoveRange(Borrowings);
            await SaveChangesAsync();

            // 2. Clear parent tables
            Equipment.RemoveRange(Equipment);
            Students.RemoveRange(Students);
            await SaveChangesAsync();

            // 3. Re-seed default data
            DatabaseSeeder.Seed(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Reset failed: {ex.Message}");
        }
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "equipment_borrowing.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EquipmentBorrowingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}