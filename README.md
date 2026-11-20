# Campus Equipment Borrowing System

A desktop application built with **.NET 10**, **Avalonia UI**, **CommunityToolkit.Mvvm**, and **Entity Framework Core** backed by **PostgreSQL 18** (`Npgsql.EntityFrameworkCore.PostgreSQL`) for managing campus equipment borrowing, returns, multi-item checkouts, student/equipment administration, and borrowing history.

## Solution Structure

* `EquipmentBorrowing.Domain`: Core domain entities (`Student`, `Equipment`, `Borrowing`), statuses, and domain validation rules.
* `EquipmentBorrowing.Application`: Application services (`BorrowEquipmentService`, `ReturnEquipmentService`, `MultiItemCheckoutService`, `StudentManagementService`, `EquipmentManagementService`, `BorrowingQueryService`), operation results, and repository interfaces.
* `EquipmentBorrowing.Infrastructure`: EF Core persistence (`EquipmentBorrowingDbContext`), Npgsql repository implementations, migrations (`20261120145808_InitialPostgreSqlSchema`), idempotent seeding (`DatabaseSeeder`), and database reset (`DatabaseResetService`).
* `EquipmentBorrowing.Desktop`: Avalonia UI views (`EquipmentView`, `BorrowingsView`), view models (`EquipmentViewModel`, `BorrowingsViewModel`, `MainWindowViewModel`), dependency injection setup, and startup migration/seeding.
* `docs/use-cases-and-traceability.md`: Use-case catalog (`UC-01`–`UC-16`), verification test suite (`TC-01`–`TC-10`), and requirements traceability matrix.

## Prerequisites

* **.NET SDK 10.0** and `dotnet ef` CLI tool (`dotnet tool install --global dotnet-ef` if not yet installed)
* **PostgreSQL 18** running locally on `localhost:5432` with the `equipment_borrowing` database created
* **Visual Studio** (or preferred .NET IDE) for running `EquipmentBorrowing.Desktop`

## Environment Configuration

The application reads the PostgreSQL connection string from the `EQUIPMENT_BORROWING_PG_CONNECTION` environment variable (`EquipmentBorrowingDbContext.ConnectionStringEnvVar`).

Set it at the **User** level in PowerShell (replace `<your_password>` with your local `postgres` password—never commit real credentials to source control):

[System.Environment]::SetEnvironmentVariable("EQUIPMENT_BORROWING_PG_CONNECTION", "Host=localhost;Port=5432;Database=equipment_borrowing;Username=postgres;Password=<your_password>", "User")

Verify the variable in PowerShell:

[System.Environment]::GetEnvironmentVariable("EQUIPMENT_BORROWING_PG_CONNECTION", "User")

> Note: If Visual Studio or a terminal was already open when you updated the environment variable, restart it so the new environment block is picked up.

## Database Migrations & Seeding

1. Apply EF Core migrations to PostgreSQL:

dotnet ef database update --project EquipmentBorrowing.Infrastructure --startup-project EquipmentBorrowing.Desktop

2. When the desktop application starts (`App.axaml.cs`), it uses `IDbContextFactory<EquipmentBorrowingDbContext>` to run `context.Database.Migrate()` and execute `DatabaseSeeder.Seed(context)` idempotently so initial sample data is populated without creating duplicates across restarts.
3. For testing, `DatabaseResetService` (accessible from the **Borrowings, Returns & Search** view via **Reset Database**) resets data and re-seeds the initial sample records.

## Running the Application

1. Open the solution in Visual Studio and set `EquipmentBorrowing.Desktop` as the startup project, or run from PowerShell:

dotnet run --project EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj

## Key Architecture & Persistence Notes

* Context Lifetime & Concurrency: Repositories use `IDbContextFactory<EquipmentBorrowingDbContext>` to create short-lived, isolated `DbContext` instances per operation rather than sharing a single long-lived context across concurrent UI/service calls.
* UTC Normalization: Npgsql maps `DateTime` properties to PostgreSQL `timestamp with time zone` (`timestamptz`). Borrowing, return, and multi-item checkout services normalize `DateTime` inputs to `DateTimeKind.Utc` before persisting.
* Atomic Multi-Item Checkout & Rollback: `MultiItemCheckoutService` validates student eligibility, active borrowing limits (`MaxActiveBorrowings = 3`), duplicate/missing selections, and item availability before calling `AddMultipleAtomicAsync`, which wraps inserts and availability updates inside an explicit database transaction (`BeginTransactionAsync` / `CommitAsync`). `BorrowEquipmentService` also reverts equipment availability if saving a borrowing record fails after marking the item borrowed.
* Database Integrity Constraints: Unique normalized indexes (`IX_Students_NormalizedStudentNumber`, `IX_Equipment_NormalizedCode`) and a filtered unique index (`IX_Borrowings_SingleActivePerEquipment` where `"Status" = 'Active'`) enforce data integrity at the database layer alongside application validation.

## Verification & Readback Check

You can verify table row counts in `equipment_borrowing` via `psql` in PowerShell:

@"
SELECT count(*) AS students FROM "Students";
SELECT count(*) AS equipment FROM "Equipment";
SELECT count(*) AS borrowings FROM "Borrowings";
"@ | & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d equipment_borrowing -h localhost -p 5432

For detailed use cases, test results, and traceability mapping, see `docs/use-cases-and-traceability.md`.

