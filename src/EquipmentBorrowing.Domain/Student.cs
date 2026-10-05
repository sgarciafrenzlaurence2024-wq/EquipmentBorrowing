namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsAllowedToBorrow { get; set; }

    private Student() { }

    public Student(int id, string name, bool isAllowedToBorrow = true)
    {
        Id = id;
        Name = name;
        IsAllowedToBorrow = isAllowedToBorrow;
    }

    public override string ToString() => Name;
}