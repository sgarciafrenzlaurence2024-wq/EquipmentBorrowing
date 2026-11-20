using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class EquipmentManagementService
{
    private readonly IEquipmentRepository _equipmentRepository;

    public EquipmentManagementService(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public async Task<OperationResult<IEnumerable<Equipment>>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _equipmentRepository.GetAllAsync(cancellationToken);
            return OperationResult<IEnumerable<Equipment>>.Success(items);
        }
        catch (Exception)
        {
            return OperationResult<IEnumerable<Equipment>>.Failure("Unable to load equipment catalog from the database.");
        }
    }

    public async Task<OperationResult<IEnumerable<Equipment>>> GetAvailableEquipmentAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _equipmentRepository.GetAvailableAsync(cancellationToken);
            return OperationResult<IEnumerable<Equipment>>.Success(items);
        }
        catch (Exception)
        {
            return OperationResult<IEnumerable<Equipment>>.Failure("Unable to load available equipment from the database.");
        }
    }

    public async Task<OperationResult<Equipment>> RegisterEquipmentAsync(
        string code,
        string name,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        var trimmedCode = (code ?? string.Empty).Trim();
        var trimmedName = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(trimmedCode))
            return OperationResult<Equipment>.ValidationFailed("Equipment identifier is required.");

        if (string.IsNullOrWhiteSpace(trimmedName))
            return OperationResult<Equipment>.ValidationFailed("Equipment name is required.");

        var normalizedCode = Equipment.NormalizeCode(trimmedCode);

        try
        {
            var duplicateExists = await _equipmentRepository.ExistsByNormalizedCodeAsync(
                normalizedCode,
                excludeEquipmentId: null,
                cancellationToken);

            if (duplicateExists)
                return OperationResult<Equipment>.ValidationFailed(
                    $"Equipment with identifier '{trimmedCode}' already exists.");

            var equipment = new Equipment(trimmedCode, trimmedName, description, isAvailable: true);
            await _equipmentRepository.AddAsync(equipment, cancellationToken);

            return OperationResult<Equipment>.Success(equipment, "Equipment registered successfully.");
        }
        catch (Exception)
        {
            return OperationResult<Equipment>.Failure("Failed to register equipment due to a database error.");
        }
    }

    public async Task<OperationResult<Equipment>> UpdateEquipmentAsync(
        int equipmentId,
        string code,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var trimmedCode = (code ?? string.Empty).Trim();
        var trimmedName = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(trimmedCode))
            return OperationResult<Equipment>.ValidationFailed("Equipment identifier is required.");

        if (string.IsNullOrWhiteSpace(trimmedName))
            return OperationResult<Equipment>.ValidationFailed("Equipment name is required.");

        var normalizedCode = Equipment.NormalizeCode(trimmedCode);

        try
        {
            var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
            if (equipment is null)
                return OperationResult<Equipment>.ValidationFailed("Selected equipment item was not found.");

            var duplicateExists = await _equipmentRepository.ExistsByNormalizedCodeAsync(
                normalizedCode,
                excludeEquipmentId: equipmentId,
                cancellationToken);

            if (duplicateExists)
                return OperationResult<Equipment>.ValidationFailed(
                    $"Another equipment item already uses identifier '{trimmedCode}'.");

            equipment.SetCode(trimmedCode);
            equipment.SetName(trimmedName);
            equipment.SetDescription(description);

            await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
            return OperationResult<Equipment>.Success(equipment, "Equipment information updated successfully.");
        }
        catch (Exception)
        {
            return OperationResult<Equipment>.Failure("Failed to update equipment due to a database error.");
        }
    }
}