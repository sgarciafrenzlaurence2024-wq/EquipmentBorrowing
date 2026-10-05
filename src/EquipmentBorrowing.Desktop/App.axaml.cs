using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public static System.IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        // Factory: repositories create a fresh, short-lived DbContext for every operation
        collection.AddDbContextFactory<EquipmentBorrowingDbContext>();

        collection.AddTransient<IEquipmentRepository, EfEquipmentRepository>();
        collection.AddTransient<IStudentRepository, EfStudentRepository>();
        collection.AddTransient<IBorrowingRepository, EfBorrowingRepository>();

        collection.AddTransient<BorrowEquipmentService>();
        collection.AddTransient<ReturnEquipmentService>();
        collection.AddTransient<EquipmentViewModel>();
        collection.AddTransient<BorrowingsViewModel>();
        collection.AddTransient<MainWindowViewModel>();

        Services = collection.BuildServiceProvider();

        try
        {
            using var context = new EquipmentBorrowingDbContext();
            context.Database.EnsureCreated();
            DatabaseSeeder.Seed(context);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DATABASE STARTUP ERROR: {ex}");
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}