using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task<int> GetActiveCountByStudentIdAsync(int studentId, CancellationToken ct = default);
    Task AddAsync(Borrowing borrowing, CancellationToken ct = default);
}