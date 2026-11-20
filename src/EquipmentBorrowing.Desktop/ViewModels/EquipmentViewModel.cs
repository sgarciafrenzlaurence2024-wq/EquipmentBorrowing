using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ObservableObject
{
    private readonly EquipmentManagementService _equipmentService;
    private readonly StudentManagementService _studentService;
    private readonly BorrowEquipmentService _borrowService;
    private readonly MultiItemCheckoutService _multiCheckoutService;

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
    private bool _showAvailableOnly;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Student Management fields (UC-12, UC-13, UC-14)
    [ObservableProperty]
    private string _studentNumberInput = string.Empty;

    [ObservableProperty]
    private string _studentNameInput = string.Empty;

    [ObservableProperty]
    private string _studentContactInput = string.Empty;

    // Equipment Management fields (UC-15, UC-16)
    [ObservableProperty]
    private string _equipmentCodeInput = string.Empty;

    [ObservableProperty]
    private string _equipmentNameInput = string.Empty;

    [ObservableProperty]
    private string _equipmentDescriptionInput = string.Empty;

    public EquipmentViewModel(
        EquipmentManagementService equipmentService,
        StudentManagementService studentService,
        BorrowEquipmentService borrowService,
        MultiItemCheckoutService multiCheckoutService)
    {
        _equipmentService = equipmentService;
        _studentService = studentService;
        _borrowService = borrowService;
        _multiCheckoutService = multiCheckoutService;
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (value is not null)
        {
            StudentNumberInput = value.StudentNumber;
            StudentNameInput = value.Name;
            StudentContactInput = value.ContactInfo ?? string.Empty;
        }
    }

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        if (value is not null)
        {
            EquipmentCodeInput = value.Code;
            EquipmentNameInput = value.Name;
            EquipmentDescriptionInput = value.Description ?? string.Empty;
        }
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            var eqResult = ShowAvailableOnly
                ? await _equipmentService.GetAvailableEquipmentAsync()
                : await _equipmentService.GetCatalogAsync();

            var stResult = await _studentService.GetAllStudentsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!eqResult.IsSuccess || eqResult.Data is null)
                {
                    Equipments = new ObservableCollection<Equipment>();
                    StatusMessage = eqResult.Message;
                }
                else
                {
                    Equipments = new ObservableCollection<Equipment>(eqResult.Data);
                }

                if (!stResult.IsSuccess || stResult.Data is null)
                {
                    Students = new ObservableCollection<Student>();
                    StatusMessage = stResult.Message;
                }
                else
                {
                    Students = new ObservableCollection<Student>(stResult.Data);
                }

                if (eqResult.IsSuccess && stResult.IsSuccess && string.IsNullOrWhiteSpace(StatusMessage))
                {
                    StatusMessage = Equipments.Count == 0
                        ? (ShowAvailableOnly ? "No available equipment found." : "Equipment catalog is empty.")
                        : string.Empty;
                }
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleAvailableFilterAsync()
    {
        ShowAvailableOnly = !ShowAvailableOnly;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (SelectedEquipment is null || SelectedStudent is null || ExpectedReturnDate is null)
        {
            StatusMessage = "Please select a student, equipment item, and expected return date.";
            return;
        }

        IsBusy = true;
        try
        {
            var checkoutDate = DateTime.Now;
            var returnDate = ExpectedReturnDate.Value.DateTime;
            var result = await _borrowService.BorrowAsync(
                SelectedStudent.Id,
                SelectedEquipment.Id,
                checkoutDate,
                returnDate);

            StatusMessage = result.Message;
            if (result.IsSuccess)
            {
                await LoadDataAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CheckoutMultipleAsync(IList? selectedItems)
    {
        if (SelectedStudent is null || ExpectedReturnDate is null)
        {
            StatusMessage = "Please select a student and expected return date for multi-item checkout.";
            return;
        }

        var ids = selectedItems?
            .OfType<Equipment>()
            .Select(e => e.Id)
            .ToList() ?? new List<int>();

        IsBusy = true;
        try
        {
            var checkoutDate = DateTime.Now;
            var returnDate = ExpectedReturnDate.Value.DateTime;
            var result = await _multiCheckoutService.CheckoutMultipleAsync(
                SelectedStudent.Id,
                ids,
                checkoutDate,
                returnDate);

            StatusMessage = result.Message;
            if (result.IsSuccess)
            {
                await LoadDataAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegisterStudentAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _studentService.RegisterStudentAsync(
                StudentNumberInput,
                StudentNameInput,
                StudentContactInput);

            StatusMessage = result.Message;
            if (result.IsSuccess)
            {
                StudentNumberInput = string.Empty;
                StudentNameInput = string.Empty;
                StudentContactInput = string.Empty;
                await LoadDataAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdateStudentAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student to update.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _studentService.UpdateStudentAsync(
                SelectedStudent.Id,
                StudentNumberInput,
                StudentNameInput,
                StudentContactInput);

            StatusMessage = result.Message;
            if (result.IsSuccess)
            {
                await LoadDataAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SetEligibilityAsync(string mode)
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student first.";
            return;
        }

        var allow = string.Equals(mode, "Permit", StringComparison.OrdinalIgnoreCase);

        IsBusy = true;
        try
        {
            var result = await _studentService.ChangeEligibilityAsync(SelectedStudent.Id, allow);
            StatusMessage = result.Message;
            if (result.IsSuccess)
            {
                await LoadDataAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegisterEquipmentAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _equipmentService.RegisterEquipmentAsync(
                EquipmentCodeInput,
                EquipmentNameInput,
                EquipmentDescriptionInput);

            StatusMessage = result.Message;
            if (result.IsSuccess)
            {
                EquipmentCodeInput = string.Empty;
                EquipmentNameInput = string.Empty;
                EquipmentDescriptionInput = string.Empty;
                await LoadDataAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdateEquipmentAsync()
    {
        if (SelectedEquipment is null)
        {
            StatusMessage = "Please select an equipment item to update.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _equipmentService.UpdateEquipmentAsync(
                SelectedEquipment.Id,
                EquipmentCodeInput,
                EquipmentNameInput,
                EquipmentDescriptionInput);

            StatusMessage = result.Message;
            if (result.IsSuccess)
            {
                await LoadDataAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}