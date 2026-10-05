using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;

    [ObservableProperty]
    private object _currentView = default!;

    public MainWindowViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;

        _ = NavigateToEquipmentAsync();
    }

    [RelayCommand]
    private async Task NavigateToEquipmentAsync()
    {
        CurrentView = _equipmentViewModel;
        await _equipmentViewModel.LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToBorrowingsAsync()
    {
        CurrentView = _borrowingsViewModel;
        await _borrowingsViewModel.LoadDataAsync();
    }
}