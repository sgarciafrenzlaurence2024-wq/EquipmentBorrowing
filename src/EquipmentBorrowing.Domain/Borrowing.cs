using System;

namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int EquipmentId { get; set; }
    public DateTime BorrowedDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public BorrowingStatus Status { get; set; }

    public Borrowing(int id, int studentId, int equipmentId, int daysToBorrow)
    {
        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        BorrowedDate = DateTime.Now;
        ExpectedReturnDate = DateTime.Now.AddDays(daysToBorrow);
        Status = BorrowingStatus.Active;
    }
}