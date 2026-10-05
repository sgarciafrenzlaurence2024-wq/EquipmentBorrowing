using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly IBorrowingRepository _borrowingRepository;

    private ObservableCollection<Borrowing> _borrowings = new();
    public ObservableCollection<Borrowing> Borrowings
    {
        get => _borrowings;
        set => SetProperty(ref _borrowings, value);
    }

    public IRelayCommand ResetDatabaseCommand { get; }

    public BorrowingsViewModel(IBorrowingRepository borrowingRepository)
    {
        _borrowingRepository = borrowingRepository;

        ResetDatabaseCommand = new AsyncRelayCommand(ResetDatabaseAsync);
        // Data is loaded by MainWindowViewModel when this view is shown
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var borrowings = await _borrowingRepository.GetAllAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Borrowings = new ObservableCollection<Borrowing>(borrowings);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB Load Error: {ex.Message}");
        }
    }

    private async Task ResetDatabaseAsync()
    {
        try
        {
            // A fresh, dedicated context just for this operation
            using var context = new EquipmentBorrowingDbContext();

            context.Borrowings.RemoveRange(context.Borrowings);
            await context.SaveChangesAsync();

            context.Equipment.RemoveRange(context.Equipment);
            context.Students.RemoveRange(context.Students);
            await context.SaveChangesAsync();

            DatabaseSeeder.Seed(context);

            // Refresh the UI list
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Reset failed: {ex.Message}");
        }
    }
}