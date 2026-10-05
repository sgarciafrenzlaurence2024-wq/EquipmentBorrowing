namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsAvailable { get; set; }

    private Equipment() { }

    public Equipment(int id, string name, bool isAvailable = true)
    {
        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }

    public void MarkAsBorrowed() => IsAvailable = false;
    public void MarkAsReturned() => IsAvailable = true;

    public override string ToString() => Name;
}