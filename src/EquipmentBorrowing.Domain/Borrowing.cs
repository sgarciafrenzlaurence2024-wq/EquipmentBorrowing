using System;

namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public int EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public DateTime BorrowedDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public BorrowingStatus Status { get; set; }

    private Borrowing() { }

    public Borrowing(int id, int studentId, int equipmentId, int daysToBorrow)
    {
        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        BorrowedDate = DateTime.UtcNow;
        ExpectedReturnDate = BorrowedDate.AddDays(daysToBorrow);
        Status = BorrowingStatus.Active;
    }

    public Borrowing(int studentId, int equipmentId, DateTime borrowedDate, DateTime expectedReturnDate)
    {
        if (expectedReturnDate <= borrowedDate)
            throw new ArgumentException("Expected return date must be later than checkout date.", nameof(expectedReturnDate));

        StudentId = studentId;
        EquipmentId = equipmentId;
        BorrowedDate = borrowedDate;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public void MarkReturned(DateTime actualReturnDate)
    {
        if (Status == BorrowingStatus.Returned)
            throw new InvalidOperationException("Borrowing has already been returned.");

        if (actualReturnDate < BorrowedDate)
            throw new ArgumentException("Actual return date cannot precede checkout date.", nameof(actualReturnDate));

        ActualReturnDate = actualReturnDate;
        Status = BorrowingStatus.Returned;
    }
}