using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ObservableObject
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly BorrowEquipmentService _borrowEquipmentService;

    [ObservableProperty]
    private ObservableCollection<Equipment> _equipments = new();

    [ObservableProperty]
    private ObservableCollection<Student> _students = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private DateTimeOffset? _expectedReturnDate = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        IStudentRepository studentRepository,
        BorrowEquipmentService borrowEquipmentService)
    {
        _equipmentRepository = equipmentRepository;
        _studentRepository = studentRepository;
        _borrowEquipmentService = borrowEquipmentService;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var equipments = await _equipmentRepository.GetAllAsync();
        var students = await _studentRepository.GetAllAsync();

        foreach (var eq in equipments) Equipments.Add(eq);
        foreach (var st in students) Students.Add(st);
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        // Presentation Validation
        if (SelectedEquipment == null || SelectedStudent == null || ExpectedReturnDate == null)
        {
            StatusMessage = "Please select a student, equipment, and date.";
            return;
        }

        // Calculate how many days the equipment will be borrowed
        int borrowDays = (ExpectedReturnDate.Value.Date - DateTime.Now.Date).Days;

        if (borrowDays <= 0)
        {
            StatusMessage = "Return date must be in the future.";
            return;
        }

        try
        {
            // Pass the calculated 'borrowDays' integer instead of a DateTime object
            bool success = await _borrowEquipmentService.ExecuteAsync(
                SelectedStudent.Id,
                SelectedEquipment.Id,
                borrowDays);

            if (success)
            {
                StatusMessage = "Successfully borrowed!";

                // Refresh list to immediately update the UI state
                Equipments.Clear();
                var updatedEquipments = await _equipmentRepository.GetAllAsync();
                foreach (var eq in updatedEquipments) Equipments.Add(eq);
            }
            else
            {
                StatusMessage = "Borrowing failed (e.g., limit reached or unavailable).";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}