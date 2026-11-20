using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class StudentManagementService
{
    private readonly IStudentRepository _studentRepository;

    public StudentManagementService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<OperationResult<IEnumerable<Student>>> GetAllStudentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var students = await _studentRepository.GetAllAsync(cancellationToken);
            return OperationResult<IEnumerable<Student>>.Success(students);
        }
        catch (Exception)
        {
            return OperationResult<IEnumerable<Student>>.Failure("Unable to load students from the database.");
        }
    }

    public async Task<OperationResult<Student>> RegisterStudentAsync(
        string studentNumber,
        string name,
        string? contactInfo = null,
        CancellationToken cancellationToken = default)
    {
        var trimmedNumber = (studentNumber ?? string.Empty).Trim();
        var trimmedName = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(trimmedNumber))
            return OperationResult<Student>.ValidationFailed("Institutional student number is required.");

        if (string.IsNullOrWhiteSpace(trimmedName))
            return OperationResult<Student>.ValidationFailed("Student full name is required.");

        var normalizedNumber = Student.NormalizeCode(trimmedNumber);

        try
        {
            var duplicateExists = await _studentRepository.ExistsByNormalizedStudentNumberAsync(
                normalizedNumber,
                excludeStudentId: null,
                cancellationToken);

            if (duplicateExists)
                return OperationResult<Student>.ValidationFailed(
                    $"A student with number '{trimmedNumber}' already exists.");

            var student = new Student(trimmedNumber, trimmedName, contactInfo, isAllowedToBorrow: true);
            await _studentRepository.AddAsync(student, cancellationToken);

            return OperationResult<Student>.Success(student, "Student registered successfully.");
        }
        catch (Exception)
        {
            return OperationResult<Student>.Failure("Failed to register student due to a database error.");
        }
    }

    public async Task<OperationResult<Student>> UpdateStudentAsync(
        int studentId,
        string studentNumber,
        string name,
        string? contactInfo,
        CancellationToken cancellationToken = default)
    {
        var trimmedNumber = (studentNumber ?? string.Empty).Trim();
        var trimmedName = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(trimmedNumber))
            return OperationResult<Student>.ValidationFailed("Institutional student number is required.");

        if (string.IsNullOrWhiteSpace(trimmedName))
            return OperationResult<Student>.ValidationFailed("Student full name is required.");

        var normalizedNumber = Student.NormalizeCode(trimmedNumber);

        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
                return OperationResult<Student>.ValidationFailed("Selected student was not found.");

            var duplicateExists = await _studentRepository.ExistsByNormalizedStudentNumberAsync(
                normalizedNumber,
                excludeStudentId: studentId,
                cancellationToken);

            if (duplicateExists)
                return OperationResult<Student>.ValidationFailed(
                    $"Another student already uses number '{trimmedNumber}'.");

            student.SetStudentNumber(trimmedNumber);
            student.SetName(trimmedName);
            student.SetContactInfo(contactInfo);

            await _studentRepository.UpdateAsync(student, cancellationToken);
            return OperationResult<Student>.Success(student, "Student information updated successfully.");
        }
        catch (Exception)
        {
            return OperationResult<Student>.Failure("Failed to update student due to a database error.");
        }
    }

    public async Task<OperationResult<Student>> ChangeEligibilityAsync(
        int studentId,
        bool allowBorrowing,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
                return OperationResult<Student>.ValidationFailed("Selected student was not found.");

            if (student.IsAllowedToBorrow == allowBorrowing)
            {
                var stateLabel = allowBorrowing ? "permitted" : "restricted";
                return OperationResult<Student>.ValidationFailed(
                    $"Student borrowing eligibility is already set to {stateLabel}.");
            }

            student.IsAllowedToBorrow = allowBorrowing;
            await _studentRepository.UpdateAsync(student, cancellationToken);

            var statusMessage = allowBorrowing
                ? "Student is now permitted to borrow equipment."
                : "Student borrowing is now restricted; existing active borrowings remain unaffected.";

            return OperationResult<Student>.Success(student, statusMessage);
        }
        catch (Exception)
        {
            return OperationResult<Student>.Failure("Failed to update student eligibility due to a database error.");
        }
    }
}