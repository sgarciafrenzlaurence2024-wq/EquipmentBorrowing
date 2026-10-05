using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.StudentId).IsRequired();
        builder.Property(b => b.EquipmentId).IsRequired();

        // Configures the foreign key relationships we defined in the ERD
        builder.HasOne<Student>()
               .WithMany()
               .HasForeignKey(b => b.StudentId);

        builder.HasOne<Equipment>()
               .WithMany()
               .HasForeignKey(b => b.EquipmentId);
    }
}