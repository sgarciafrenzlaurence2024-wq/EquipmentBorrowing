using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public static System.IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        var collection = new ServiceCollection();

        // Register the single mock database for all three interfaces
        var db = new EquipmentBorrowing.Infrastructure.Repositories.InMemoryRepositories();
        collection.AddSingleton<EquipmentBorrowing.Application.Interfaces.IEquipmentRepository>(db);
        collection.AddSingleton<EquipmentBorrowing.Application.Interfaces.IStudentRepository>(db);
        collection.AddSingleton<EquipmentBorrowing.Application.Interfaces.IBorrowingRepository>(db);

        // Register Services and ViewModels
        collection.AddTransient<EquipmentBorrowing.Application.Services.BorrowEquipmentService>();
        collection.AddTransient<EquipmentBorrowing.Application.Services.ReturnEquipmentService>();
        collection.AddTransient<EquipmentViewModel>();
        collection.AddTransient<BorrowingsViewModel>();

        Services = collection.BuildServiceProvider();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}