using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly ReturnEquipmentService _returnService;
    private readonly IBorrowingRepository _borrowingRepo;

    [ObservableProperty] private ObservableCollection<Borrowing> _activeBorrowings = new();
    [ObservableProperty] private Borrowing? _selectedBorrowing;
    [ObservableProperty] private string? _statusMessage;

    public BorrowingsViewModel(ReturnEquipmentService returnService, IBorrowingRepository borrowingRepo)
    {
        _returnService = returnService;
        _borrowingRepo = borrowingRepo;
        _ = LoadBorrowingsAsync();
    }

    public async Task LoadBorrowingsAsync()
    {
        ActiveBorrowings.Clear();
        foreach (var b in await _borrowingRepo.GetAllAsync())
        {
            if (b.Status == BorrowingStatus.Active)
                ActiveBorrowings.Add(b);
        }
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (SelectedBorrowing == null)
        {
            StatusMessage = "Validation Error: Select an active borrowing.";
            return;
        }

        bool success = await _returnService.ExecuteAsync(SelectedBorrowing.Id);

        if (success)
        {
            StatusMessage = $"Success: Returned Item ID {SelectedBorrowing.EquipmentId}.";
            await LoadBorrowingsAsync();
        }
        else
        {
            StatusMessage = "Failed: Record not found.";
        }
    }
}