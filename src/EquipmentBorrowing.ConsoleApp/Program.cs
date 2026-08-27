using System;
using System.Threading.Tasks;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Repositories;

Console.WriteLine("=== CAMPUS EQUIPMENT BORROWING DEMO (UPDATED) ===\n");

var db = new InMemoryRepositories();

// Seed initial data
db.Students.Add(new Student(1, "Alice", isAllowedToBorrow: true));

db.Equipments.Add(new Equipment(101, "Oscilloscope", isAvailable: true));
db.Equipments.Add(new Equipment(102, "Multimeter", isAvailable: true));
db.Equipments.Add(new Equipment(103, "Soldering Iron", isAvailable: true));
db.Equipments.Add(new Equipment(104, "Power Supply", isAvailable: true));

// Initialize both services
var borrowService = new BorrowEquipmentService(db, db, db);
var returnService = new ReturnEquipmentService(db, db);

// Test 1: Max Borrowing Limit
Console.WriteLine("--- TEST 1: Max Borrowing Limit Check ---");
await borrowService.ExecuteAsync(studentId: 1, equipmentId: 101, days: 3);
await borrowService.ExecuteAsync(studentId: 1, equipmentId: 102, days: 3);
await borrowService.ExecuteAsync(studentId: 1, equipmentId: 103, days: 3);
Console.WriteLine("Alice successfully borrowed 3 items (Oscilloscope, Multimeter, Soldering Iron).");

bool overLimitResult = await borrowService.ExecuteAsync(studentId: 1, equipmentId: 104, days: 3);
Console.WriteLine($"Alice requesting 4th item (Power Supply): {overLimitResult} (Expected: False)\n");

// Test 2: Returning Equipment
Console.WriteLine("--- TEST 2: Returning Equipment ---");
var activeBorrowing = db.Borrowings[0]; // Grab Alice's first borrowing record
Console.WriteLine($"Attempting to return Borrowing ID {activeBorrowing.Id} (Oscilloscope)...");

bool returnResult = await returnService.ExecuteAsync(activeBorrowing.Id);
Console.WriteLine($"Return processed successfully: {returnResult} (Expected: True)");
Console.WriteLine($"Oscilloscope availability status: {db.Equipments[0].IsAvailable} (Expected: True)\n");

// Test 3: Borrowing Again After Return
Console.WriteLine("--- TEST 3: Borrowing After Slot Opens ---");
bool retryResult = await borrowService.ExecuteAsync(studentId: 1, equipmentId: 104, days: 3);
Console.WriteLine($"Alice requesting Power Supply again: {retryResult} (Expected: True)\n");