using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(Equipment equipment, CancellationToken ct = default);
}