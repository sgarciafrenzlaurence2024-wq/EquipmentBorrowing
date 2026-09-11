using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}