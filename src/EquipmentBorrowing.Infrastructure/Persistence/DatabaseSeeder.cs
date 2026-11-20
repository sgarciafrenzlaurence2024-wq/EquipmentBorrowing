using System;
using System.Linq;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static void Seed(EquipmentBorrowingDbContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        if (!context.Students.Any())
        {
            context.Students.AddRange(
                new Student("STU-0001", "Frenz Laurence", "frenz@campus.edu", isAllowedToBorrow: true),
                new Student("STU-0002", "Katyy Dyy", "katyy@campus.edu", isAllowedToBorrow: true),
                new Student("STU-0003", "Kirzten Sunshine", null, isAllowedToBorrow: false)
            );
        }

        if (!context.Equipment.Any())
        {
            context.Equipment.AddRange(
                new Equipment("EQ-0001", "Sony Projector", "Classroom HDMI projector", isAvailable: true),
                new Equipment("EQ-0002", "Dell XPS Laptop", "Lab presentation laptop", isAvailable: true),
                new Equipment("EQ-0003", "Wireless Microphone", "Handheld wireless microphone", isAvailable: true)
            );
        }

        context.SaveChanges();
    }
}