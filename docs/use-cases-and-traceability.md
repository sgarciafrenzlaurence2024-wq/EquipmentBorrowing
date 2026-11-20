# Use-Case Catalog, Verification Suite & Traceability Matrix

## 1. Use-Case Catalog (`UC-01` – `UC-16`)

### Catalog, Checkout & Returns
* **UC-01 – View Equipment Catalog**: Display equipment items with code, name, description, and availability status.
* **UC-02 – Filter Available Equipment**: Toggle **Available Only** to filter the catalog down to items currently available for borrowing.
* **UC-03 – Borrow Single Equipment Item**: Borrow an available item for an eligible student up to the active borrowing limit (`MaxActiveBorrowings = 3`) with a future expected return date normalized to UTC.
* **UC-04 – Enforce Borrowing Eligibility & Availability Rules**: Reject borrowing when the student does not exist, is restricted (`!IsAllowedToBorrow`), has reached the max active borrowing limit (`3`), or when the equipment is missing or already unavailable (`!IsAvailable`).
* **UC-05 – Return Borrowed Equipment**: Mark an active borrowing as `Returned` with a UTC return date and restore the associated equipment item to available.
* **UC-06 – Multi-Item Checkout**: Checkout two or more distinct available items for an eligible student atomically (`AddMultipleAtomicAsync`), rolling back on failure so no partial checkouts remain.

### Histories, Search & Database Maintenance
* **UC-07 – View Student Borrowing History**: Query borrowing records associated with a selected student (`GetByStudentIdAsync`).
* **UC-08 – View Equipment Borrowing History**: Query borrowing records associated with a selected equipment item (`GetByEquipmentIdAsync`).
* **UC-09 – Search, Filter & Sort Borrowings**: Search borrowings by student name, student number, or equipment name; filter by status (`All`, `Active`, `Returned`); and sort by `BorrowedDate` or `ExpectedReturnDate` ascending/descending.
* **UC-10 – Non-Destructive Startup Seeding & Migration**: Apply pending EF Core migrations (`20261120145808_InitialPostgreSqlSchema`) and seed initial students and equipment idempotently across restarts without creating duplicates.
* **UC-11 – Reset Database**: Reset database records and re-seed initial sample data cleanly (`IDatabaseResetService` / `DatabaseResetService`).

### Student & Equipment Management
* **UC-12 – Register Student**: Add a new student record with unique normalized student number, name, and optional contact info.
* **UC-13 – Update Student Details**: Update an existing student's details while enforcing unique normalized student number rules.
* **UC-14 – Permit / Restrict Student Borrowing**: Toggle a student's `IsAllowedToBorrow` eligibility flag (`Permit` / `Restrict`).
* **UC-15 – Register Equipment Item**: Add a new equipment record with unique normalized equipment code, name, and description.
* **UC-16 – Update Equipment Item**: Update an existing equipment item's details while enforcing unique normalized code rules.

## 2. Verification Test Suite & Observed Results

| Test ID | Use Case | Scenario / Action | Expected Result | Observed Status |
| :--- | :--- | :--- | :--- | :--- |
| **TC-01** | UC-10 | Run `dotnet ef database update` with `InitialPostgreSqlSchema` and launch app twice | Schema created cleanly (`Students`, `Equipment`, `Borrowings`, `__EFMigrationsHistory`); 3 students & 3 equipment items seeded without duplicates | **Passed** |
| **TC-02** | UC-02 | Toggle **Available Only** on **Equipment Catalog** | Filters catalog to available items only | **Passed** |
| **TC-03** | UC-04 | Attempt to borrow with restricted student (`Kirzten Sunshine` / `STU-0003`) | Rejected with *"Selected student is currently restricted from borrowing equipment."* and no borrowing created | **Passed** |
| **TC-04** | UC-04 | Attempt to borrow an already-borrowed/unavailable item | Rejected with *"Selected equipment is currently unavailable."* | **Passed** |
| **TC-05** | UC-03 | Borrow an available item for eligible student (`Frenz Laurence`) with future return date | Succeeds (*"Equipment borrowed successfully."*), item becomes unavailable, active row appears in Borrowings view | **Passed** |
| **TC-06** | UC-05 | Return active borrowing in **Borrowings, Returns & Search** | Borrowing marked returned, active list clears (*"No active borrowings found."*), item becomes available again | **Passed** |
| **TC-07** | UC-12 | Attempt to register duplicate student number (`STU-0002`) | Rejected with *"A student with number 'STU-0002' already exists."* | **Passed** |
| **TC-08** | UC-07 | View borrowing history for `Kirzten Sunshine` | Displays *"No borrowing history for Kirzten Sunshine."* when none exist; lists matching records when present | **Passed** |
| **TC-09** | UC-11 | Click **Reset Database** and verify via `psql` readback | Resets state and re-seeds cleanly (`3` students, `3` equipment, `0` borrowings) | **Passed** |
| **TC-10** | UC-06 | Select `2+` available items and click **Checkout Multi (2+)** (plus failure/rollback check) | Atomically creates borrowings and marks items unavailable; rolls back cleanly on invalid/failed batch | **Ready / Pending final multi-item capture** |

## 3. Requirements & Architecture Traceability Matrix

| Use Case | Primary Components (`Application` / `Domain` / `Infrastructure` / `Desktop`) | Persistence / Rule Mechanism | Test Cases |
| :--- | :--- | :--- | :--- |
| **UC-01, UC-02** | `EquipmentViewModel`, `EquipmentView`, `IEquipmentRepository` / `EfEquipmentRepository` | `Equipment.IsAvailable` & active borrowing check in `GetAvailableAsync` | TC-02 |
| **UC-03, UC-04** | `BorrowEquipmentService`, `Borrowing`, `Equipment`, `Student` | `MaxActiveBorrowings = 3`, eligibility & availability checks, UTC normalization, rollback guard | TC-03, TC-04, TC-05 |
| **UC-05** | `ReturnEquipmentService`, `Borrowing`, `Equipment` | `Borrowing.MarkReturned(utcActualReturnDate)` & `Equipment.MarkAsReturned()` | TC-06 |
| **UC-06** | `MultiItemCheckoutService`, `IBorrowingRepository.AddMultipleAtomicAsync` | Explicit transaction (`BeginTransactionAsync` / `CommitAsync`), count/duplicate/availability validation | TC-10 |
| **UC-07, UC-08, UC-09** | `BorrowingQueryService`, `BorrowingsViewModel`, `BorrowingsView`, `EfBorrowingRepository` | `GetByStudentIdAsync`, `GetByEquipmentIdAsync`, `SearchAsync` (`ILIKE`, status filter, sort field/direction) | TC-08 |
| **UC-10, UC-11** | `App.axaml.cs`, `EquipmentBorrowingDbContext`, `DatabaseSeeder`, `DatabaseResetService` | `IDbContextFactory<EquipmentBorrowingDbContext>`, `Database.Migrate()`, idempotent seeding, reset service | TC-01, TC-09 |
| **UC-12–UC-16** | `StudentManagementService`, `EquipmentManagementService`, `EfStudentRepository`, `EfEquipmentRepository` | Unique normalized indexes (`IX_Students_NormalizedStudentNumber`, `IX_Equipment_NormalizedCode`) & eligibility flag | TC-07 |
