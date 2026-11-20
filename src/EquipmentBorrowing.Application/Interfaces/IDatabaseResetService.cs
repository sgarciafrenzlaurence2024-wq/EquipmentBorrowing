using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Services;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IDatabaseResetService
{
    Task<OperationResult> ResetAndSeedAsync(CancellationToken cancellationToken = default);
}