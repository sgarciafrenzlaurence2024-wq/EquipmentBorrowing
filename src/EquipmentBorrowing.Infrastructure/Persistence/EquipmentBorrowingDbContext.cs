using System;
using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContext : DbContext
{
    public const string ConnectionStringEnvVar = "EQUIPMENT_BORROWING_PG_CONNECTION";

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EquipmentBorrowingDbContext).Assembly);

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.StudentNumber).HasMaxLength(64).IsRequired();
            entity.Property(s => s.NormalizedStudentNumber).HasMaxLength(64).IsRequired();
            entity.Property(s => s.Name).HasMaxLength(200).IsRequired();
            entity.Property(s => s.ContactInfo).HasMaxLength(200);
            entity.Property(s => s.IsAllowedToBorrow).IsRequired();

            entity.HasIndex(s => s.NormalizedStudentNumber)
                .IsUnique()
                .HasDatabaseName("IX_Students_NormalizedStudentNumber");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(64).IsRequired();
            entity.Property(e => e.NormalizedCode).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsAvailable).IsRequired();

            entity.HasIndex(e => e.NormalizedCode)
                .IsUnique()
                .HasDatabaseName("IX_Equipment_NormalizedCode");
        });

        modelBuilder.Entity<Borrowing>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.BorrowedDate).IsRequired();
            entity.Property(b => b.ExpectedReturnDate).IsRequired();
            entity.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.HasOne(b => b.Student)
                .WithMany()
                .HasForeignKey(b => b.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Equipment)
                .WithMany()
                .HasForeignKey(b => b.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enforces at most one Active borrowing row per EquipmentId in PostgreSQL
            entity.HasIndex(b => b.EquipmentId)
                .IsUnique()
                .HasFilter("\"Status\" = 'Active'")
                .HasDatabaseName("IX_Borrowings_SingleActivePerEquipment");
        });

        base.OnModelCreating(modelBuilder);
    }
}