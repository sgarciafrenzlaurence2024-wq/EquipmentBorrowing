-- 1. Basic Retrieval: Retrieve all equipment
SELECT * 
FROM Equipment;

-- 2. Filtering: Retrieve only currently available equipment
-- (Assuming 1 represents 'true' for SQLite booleans)
SELECT * 
FROM Equipment 
WHERE IsAvailable = 1;

-- 3. Join: Retrieve active borrowings with student and equipment information
-- (Assuming Status 0 or 1 represents 'Active' in your BorrowingStatus enum)
SELECT 
    s.Name AS StudentName, 
    e.Name AS EquipmentName, 
    b.DurationDays, 
    b.Status
FROM Borrowings b
INNER JOIN Students s ON b.StudentId = s.Id
INNER JOIN Equipment e ON b.EquipmentId = e.Id
WHERE b.Status = 1;

-- 4. Aggregate: Count the number of active borrowings per student
SELECT 
    s.Name AS StudentName, 
    COUNT(b.Id) AS ActiveBorrowingCount
FROM Students s
LEFT JOIN Borrowings b ON s.Id = b.StudentId AND b.Status = 1
GROUP BY s.Id, s.Name;

-- 5. Update: Change an equipment's availability status
UPDATE Equipment 
SET IsAvailable = 0 
WHERE Id = 2;