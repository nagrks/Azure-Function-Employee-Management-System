-- SQL Script to create the employees table
-- Run this script in your MySQL database (ems)

CREATE TABLE IF NOT EXISTS employees (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Position VARCHAR(255) NOT NULL,
    Salary DECIMAL(10, 2) NOT NULL,
    HireDate DATETIME NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Sample data (optional)
-- INSERT INTO employees (Name, Email, Position, Salary, HireDate) VALUES
-- ('John Doe', 'john@example.com', 'Software Engineer', 75000, '2023-01-15'),
-- ('Jane Smith', 'jane@example.com', 'Manager', 85000, '2022-06-20');
