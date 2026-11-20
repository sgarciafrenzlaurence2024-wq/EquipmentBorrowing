using System;

namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; private set; }
    public string StudentNumber { get; private set; } = string.Empty;
    public string NormalizedStudentNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ContactInfo { get; private set; }
    public bool IsAllowedToBorrow { get; set; } = true;

    private Student() { }

    public Student(int id, string name, bool isAllowedToBorrow = true)
    {
        Id = id;
        StudentNumber = $"STU-{id:D4}";
        NormalizedStudentNumber = NormalizeCode(StudentNumber);
        Name = (name ?? string.Empty).Trim();
        IsAllowedToBorrow = isAllowedToBorrow;
    }

    public Student(string studentNumber, string name, string? contactInfo = null, bool isAllowedToBorrow = true)
    {
        SetStudentNumber(studentNumber);
        SetName(name);
        SetContactInfo(contactInfo);
        IsAllowedToBorrow = isAllowedToBorrow;
    }

    public void SetStudentNumber(string studentNumber)
    {
        var trimmed = (studentNumber ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Student number is required.", nameof(studentNumber));

        StudentNumber = trimmed;
        NormalizedStudentNumber = NormalizeCode(trimmed);
    }

    public void SetName(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Student name is required.", nameof(name));

        Name = trimmed;
    }

    public void SetContactInfo(string? contactInfo)
    {
        if (contactInfo is null)
        {
            ContactInfo = null;
            return;
        }

        var trimmed = contactInfo.Trim();
        ContactInfo = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public static string NormalizeCode(string value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant();

    public override string ToString() => $"{Name} ({StudentNumber})";
}