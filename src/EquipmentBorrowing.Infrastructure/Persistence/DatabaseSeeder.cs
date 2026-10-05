using System.Linq;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static void Seed(EquipmentBorrowingDbContext context)
    {
        // Only inject data if the Students table is completely empty
        if (!context.Students.Any())
        {
            context.Students.AddRange(
                new Student(1, "Alice Smith"),
                new Student(2, "Bob Johnson"),
                new Student(3, "Charlie Brown")
            );
        }

        // Only inject data if the Equipment table is completely empty
        if (!context.Equipment.Any())
        {
            context.Equipment.AddRange(
                new Equipment(1, "Sony Projector"),
                new Equipment(2, "Dell XPS Laptop"),
                new Equipment(3, "Wireless Microphone")
            );
        }

        // Save these initial records to the SQLite file
        context.SaveChanges();
    }
}