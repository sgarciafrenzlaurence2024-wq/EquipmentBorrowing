using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
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
    public ObservableCollection<Equipment> EquipmentList => Equipments;

    [ObservableProperty]
    private ObservableCollection<Student> _students = new();
    public ObservableCollection<Student> StudentList => Students;

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
        // Data is loaded by MainWindowViewModel when this view is shown
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var equipments = await _equipmentRepository.GetAllAsync();
            var students = await _studentRepository.GetAllAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Equipments = new ObservableCollection<Equipment>(equipments);
                Students = new ObservableCollection<Student>(students);

                if (Equipments.Count == 0)
                {
                    Equipments.Add(new Equipment(999, "⚠️ Database is empty!"));
                }
                if (Students.Count == 0)
                {
                    Students.Add(new Student(999, "⚠️ Database is empty!"));
                }
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"DB Load Error: {ex.Message}";
            });
        }
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (SelectedEquipment == null || SelectedStudent == null || ExpectedReturnDate == null)
        {
            StatusMessage = "Please select a student, equipment, and date.";
            return;
        }

        int borrowDays = (ExpectedReturnDate.Value.Date - DateTime.Now.Date).Days;

        if (borrowDays <= 0)
        {
            StatusMessage = "Return date must be in the future.";
            return;
        }

        try
        {
            bool success = await _borrowEquipmentService.ExecuteAsync(
                SelectedStudent.Id,
                SelectedEquipment.Id,
                borrowDays);

            if (success)
            {
                StatusMessage = "Successfully borrowed!";
                var updatedEquipments = await _equipmentRepository.GetAllAsync();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Equipments = new ObservableCollection<Equipment>(updatedEquipments);
                });
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