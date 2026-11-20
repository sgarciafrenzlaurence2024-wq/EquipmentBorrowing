using System;

namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NormalizedCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsAvailable { get; set; } = true;

    private Equipment() { }

    public Equipment(int id, string name, bool isAvailable = true)
    {
        Id = id;
        Code = $"EQ-{id:D4}";
        NormalizedCode = NormalizeCode(Code);
        Name = (name ?? string.Empty).Trim();
        IsAvailable = isAvailable;
    }

    public Equipment(string code, string name, string? description = null, bool isAvailable = true)
    {
        SetCode(code);
        SetName(name);
        SetDescription(description);
        IsAvailable = isAvailable;
    }

    public void SetCode(string code)
    {
        var trimmed = (code ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Equipment identifier is required.", nameof(code));

        Code = trimmed;
        NormalizedCode = NormalizeCode(trimmed);
    }

    public void SetName(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Equipment name is required.", nameof(name));

        Name = trimmed;
    }

    public void SetDescription(string? description)
    {
        if (description is null)
        {
            Description = null;
            return;
        }

        var trimmed = description.Trim();
        Description = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public void MarkAsBorrowed() => IsAvailable = false;
    public void MarkAsReturned() => IsAvailable = true;

    public static string NormalizeCode(string value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant();

    public override string ToString() => $"{Name} ({Code})";
}