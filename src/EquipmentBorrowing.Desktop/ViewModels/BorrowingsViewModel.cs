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

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly BorrowingQueryService _queryService;
    private readonly ReturnEquipmentService _returnService;
    private readonly StudentManagementService _studentService;
    private readonly EquipmentManagementService _equipmentService;
    private readonly IDatabaseResetService _resetService;

    [ObservableProperty]
    private ObservableCollection<Borrowing> _borrowings = new();

    [ObservableProperty]
    private Borrowing? _selectedBorrowing;

    [ObservableProperty]
    private ObservableCollection<Student> _students = new();

    [ObservableProperty]
    private Student? _historyStudent;

    [ObservableProperty]
    private ObservableCollection<Equipment> _equipments = new();

    [ObservableProperty]
    private Equipment? _historyEquipment;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _statusFilterIndex;

    [ObservableProperty]
    private int _sortFieldIndex;

    [ObservableProperty]
    private int _sortDirectionIndex = 1;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public BorrowingsViewModel(
        BorrowingQueryService queryService,
        ReturnEquipmentService returnService,
        StudentManagementService studentService,
        EquipmentManagementService equipmentService,
        IDatabaseResetService resetService)
    {
        _queryService = queryService;
        _returnService = returnService;
        _studentService = studentService;
        _equipmentService = equipmentService;
        _resetService = resetService;
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            var stResult = await _studentService.GetAllStudentsAsync();
            var eqResult = await _equipmentService.GetCatalogAsync();
            var brResult = await _queryService.GetActiveBorrowingsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (stResult.IsSuccess && stResult.Data is not null)
                    Students = new ObservableCollection<Student>(stResult.Data);

                if (eqResult.IsSuccess && eqResult.Data is not null)
                    Equipments = new ObservableCollection<Equipment>(eqResult.Data);

                if (!brResult.IsSuccess || brResult.Data is null)
                {
                    Borrowings = new ObservableCollection<Borrowing>();
                    StatusMessage = brResult.Message;
                }
                else
                {
                    Borrowings = new ObservableCollection<Borrowing>(brResult.Data);
                    StatusMessage = Borrowings.Count == 0 ? "No active borrowings found." : string.Empty;
                }
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReturnSelectedAsync()
    {
        if (SelectedBorrowing is null)
        {
            StatusMessage = "Please select a borrowing record to return.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _returnService.ReturnAsync(SelectedBorrowing.Id, DateTime.Now);
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
    private async Task SearchBorrowingsAsync()
    {
        BorrowingStatus? filter = StatusFilterIndex switch
        {
            1 => BorrowingStatus.Active,
            2 => BorrowingStatus.Returned,
            _ => null
        };

        var sortField = SortFieldIndex == 1
            ? BorrowingSortField.ExpectedReturnDate
            : BorrowingSortField.BorrowedDate;

        var sortDirection = SortDirectionIndex == 0
            ? SortDirection.Ascending
            : SortDirection.Descending;

        IsBusy = true;
        try
        {
            var result = await _queryService.SearchBorrowingsAsync(
                SearchText,
                filter,
                sortField,
                sortDirection);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!result.IsSuccess || result.Data is null)
                {
                    Borrowings = new ObservableCollection<Borrowing>();
                    StatusMessage = result.Message;
                }
                else
                {
                    Borrowings = new ObservableCollection<Borrowing>(result.Data);
                    StatusMessage = Borrowings.Count == 0 ? "No matching borrowing records found." : string.Empty;
                }
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        StatusFilterIndex = 0;
        SortFieldIndex = 0;
        SortDirectionIndex = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadStudentHistoryAsync()
    {
        if (HistoryStudent is null)
        {
            StatusMessage = "Please select a student to view borrowing history.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _queryService.GetStudentHistoryAsync(HistoryStudent.Id);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!result.IsSuccess || result.Data is null)
                {
                    Borrowings = new ObservableCollection<Borrowing>();
                    StatusMessage = result.Message;
                }
                else
                {
                    Borrowings = new ObservableCollection<Borrowing>(result.Data);
                    StatusMessage = Borrowings.Count == 0
                        ? $"No borrowing history for {HistoryStudent.Name}."
                        : $"Showing {Borrowings.Count} record(s) for {HistoryStudent.Name}.";
                }
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadEquipmentHistoryAsync()
    {
        if (HistoryEquipment is null)
        {
            StatusMessage = "Please select an equipment item to view borrowing history.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _queryService.GetEquipmentHistoryAsync(HistoryEquipment.Id);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!result.IsSuccess || result.Data is null)
                {
                    Borrowings = new ObservableCollection<Borrowing>();
                    StatusMessage = result.Message;
                }
                else
                {
                    Borrowings = new ObservableCollection<Borrowing>(result.Data);
                    StatusMessage = Borrowings.Count == 0
                        ? $"No borrowing history for {HistoryEquipment.Name}."
                        : $"Showing {Borrowings.Count} record(s) for {HistoryEquipment.Name}.";
                }
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetDatabaseAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _resetService.ResetAndSeedAsync();
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