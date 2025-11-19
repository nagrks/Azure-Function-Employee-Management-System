# Employee Management Azure Function

This Azure Function HTTP Trigger connects to a MySQL database (ems) and provides CRUD operations for employee records.

## Prerequisites

1. **MySQL Server**: Ensure MySQL is running on localhost:3306
2. **.NET 8.0**: Required to run this Azure Function
3. **Azure Functions Core Tools**: For local testing

## Database Setup

1. Create the MySQL database:
   ```sql
   CREATE DATABASE ems;
   ```

2. Run the provided SQL script (`setup.sql`) to create the employees table:
   ```bash
   mysql -u root -p ems < setup.sql
   ```

3. Update the connection string in `MyHttpTrigger.cs` if needed:
   ```csharp
   private static readonly string ConnectionString = "Server=localhost;Database=ems;Uid=root;Pwd=;";
   ```

## API Endpoints

### Get All Employees
- **Method**: GET
- **URL**: `http://localhost:7071/api/employee`
- **Response**: Array of employee objects

### Get Employee by ID
- **Method**: GET
- **URL**: `http://localhost:7071/api/employee/{id}`
- **Response**: Single employee object

### Create Employee
- **Method**: POST
- **URL**: `http://localhost:7071/api/employee`
- **Body**:
  ```json
  {
    "name": "John Doe",
    "email": "john@example.com",
    "position": "Software Engineer",
    "salary": 75000,
    "hireDate": "2024-01-15T00:00:00"
  }
  ```
- **Response**: Created employee object with generated ID

### Update Employee
- **Method**: PUT
- **URL**: `http://localhost:7071/api/employee/{id}`
- **Body**:
  ```json
  {
    "name": "John Doe",
    "email": "john.doe@example.com",
    "position": "Senior Software Engineer",
    "salary": 95000,
    "hireDate": "2024-01-15T00:00:00"
  }
  ```
- **Response**: Success message

### Delete Employee
- **Method**: DELETE
- **URL**: `http://localhost:7071/api/employee/{id}`
- **Response**: Success message

## Running Locally

1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

2. Start the Azure Function:
   ```bash
   func start
   ```

3. Test the endpoints using Postman, curl, or any HTTP client.

## File Structure

- `MyHttpTrigger.cs` - Main Azure Function HTTP trigger with routing logic
- `Models/Employee.cs` - Employee data model
- `Services/EmployeeService.cs` - Database service for employee operations
- `setup.sql` - SQL script for database initialization
- `MyProjFolder.csproj` - Project file with dependencies

## Dependencies

- Microsoft.NET.Sdk.Functions (v4.6.0)
- MySql.Data (v8.3.0)
- Newtonsoft.Json (included with Azure Functions)

## Notes

- Connection string is currently hardcoded. For production, use Azure Key Vault or environment variables.
- All SQL queries use parameterized queries to prevent SQL injection.
- Timestamps are automatically managed by the database.
