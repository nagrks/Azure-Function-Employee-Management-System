-- SQL Script to create the az_employees table
-- Run this script in your SQL Server database (ems)

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'az_employees')
BEGIN
    CREATE TABLE az_employees (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name VARCHAR(255) NOT NULL,
        Email VARCHAR(255) NOT NULL,
        Position VARCHAR(255) NOT NULL,
        Salary DECIMAL(10, 2) NOT NULL,
        HireDate DATETIME NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE()
    );
END

-- Sample data (optional)
-- INSERT INTO az_employees (Name, Email, Position, Salary, HireDate) VALUES
-- ('John Doe', 'john@example.com', 'Software Engineer', 75000, '2023-01-15'),
-- ('Jane Smith', 'jane@example.com', 'Manager', 85000, '2022-06-20');
