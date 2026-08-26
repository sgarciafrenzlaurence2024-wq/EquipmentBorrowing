# Campus Equipment Borrowing System - Architecture

## 1. Solution Structure
* Domain: Contains problem concepts (Student, Equipment, Borrowing) and rules.
* Application: Contains use cases (BorrowEquipmentService) and repository interfaces.
* Infrastructure: Contains technical data storage (InMemoryRepositories).
* ConsoleApp: The executable project wiring dependencies together.

## 2. Dependency Direction
ConsoleApp -> Infrastructure -> Application -> Domain

## 3. Use Case Mapping
* Actor: Student
* Use Case: Borrow Equipment
* Application Service: BorrowEquipmentService
* Domain Objects: Student, Equipment, Borrowing
* Interfaces: IStudentRepository, IEquipmentRepository, IBorrowingRepository
* Infrastructure: InMemoryRepositories

## 4. Reflection Answers
* Why depend on interfaces? Decouples business logic from data storage, allowing independent testing and replacement.
* What remains unchanged for SQLite? Domain and Application projects remain 100% untouched.
* Where will Avalonia go? In a separate presentation project (e.g., EquipmentBorrowing.Desktop).
* Should UI execute queries? No. UI must delegate rule validation to Application Services.
* Business Operation: The ExecuteAsync method inside BorrowEquipmentService represents the actual business operation.
