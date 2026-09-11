using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object _currentView;

    public MainWindowViewModel()
    {
        // Set the default launch screen
        _currentView = App.Services?.GetRequiredService<EquipmentViewModel>()!;
    }

    [RelayCommand]
    private void NavigateToEquipment()
    {
        CurrentView = App.Services?.GetRequiredService<EquipmentViewModel>()!;
    }

    [RelayCommand]
    private void NavigateToBorrowings()
    {
        CurrentView = App.Services?.GetRequiredService<BorrowingsViewModel>()!;
    }
}