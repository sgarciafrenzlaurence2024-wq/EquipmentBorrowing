using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        var connectionString = Environment.GetEnvironmentVariable(EquipmentBorrowingDbContext.ConnectionStringEnvVar)
            ?? throw new InvalidOperationException(
                $"Missing PostgreSQL connection string. Set the '{EquipmentBorrowingDbContext.ConnectionStringEnvVar}' environment variable.");

        collection.AddDbContextFactory<EquipmentBorrowingDbContext>(options =>
            options.UseNpgsql(connectionString));

        collection.AddTransient<IEquipmentRepository, EfEquipmentRepository>();
        collection.AddTransient<IStudentRepository, EfStudentRepository>();
        collection.AddTransient<IBorrowingRepository, EfBorrowingRepository>();
        collection.AddTransient<IDatabaseResetService, DatabaseResetService>();

        collection.AddTransient<BorrowEquipmentService>();
        collection.AddTransient<ReturnEquipmentService>();
        collection.AddTransient<MultiItemCheckoutService>();
        collection.AddTransient<StudentManagementService>();
        collection.AddTransient<EquipmentManagementService>();
        collection.AddTransient<BorrowingQueryService>();

        collection.AddTransient<EquipmentViewModel>();
        collection.AddTransient<BorrowingsViewModel>();
        collection.AddTransient<MainWindowViewModel>();

        Services = collection.BuildServiceProvider();

        try
        {
            var factory = Services.GetRequiredService<IDbContextFactory<EquipmentBorrowingDbContext>>();
            using var context = factory.CreateDbContext();
            context.Database.Migrate();
            DatabaseSeeder.Seed(context);
        }
        catch (Exception ex)
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