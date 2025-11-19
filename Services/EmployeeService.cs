using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MyProjFolder.Models;

namespace MyProjFolder.Services
{
    public class EmployeeService
    {
        private readonly string _connectionString;

        public EmployeeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            var employees = new List<Employee>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, Name, Email, Position, Salary, HireDate FROM employees";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            employees.Add(new Employee
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                Email = reader.GetString("Email"),
                                Position = reader.GetString("Position"),
                                Salary = reader.GetDecimal("Salary"),
                                HireDate = reader.GetDateTime("HireDate")
                            });
                        }
                    }
                }
            }

            return employees;
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, Name, Email, Position, Salary, HireDate FROM employees WHERE Id = @Id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Employee
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                Email = reader.GetString("Email"),
                                Position = reader.GetString("Position"),
                                Salary = reader.GetDecimal("Salary"),
                                HireDate = reader.GetDateTime("HireDate")
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<int> CreateEmployeeAsync(Employee employee)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"INSERT INTO employees (Name, Email, Position, Salary, HireDate) 
                                VALUES (@Name, @Email, @Position, @Salary, @HireDate);
                                SELECT LAST_INSERT_ID();";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", employee.Name ?? "");
                    command.Parameters.AddWithValue("@Email", employee.Email ?? "");
                    command.Parameters.AddWithValue("@Position", employee.Position ?? "");
                    command.Parameters.AddWithValue("@Salary", employee.Salary);
                    command.Parameters.AddWithValue("@HireDate", employee.HireDate);
                    
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"UPDATE employees 
                               SET Name = @Name, Email = @Email, Position = @Position, 
                                   Salary = @Salary, HireDate = @HireDate 
                               WHERE Id = @Id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", employee.Id);
                    command.Parameters.AddWithValue("@Name", employee.Name ?? "");
                    command.Parameters.AddWithValue("@Email", employee.Email ?? "");
                    command.Parameters.AddWithValue("@Position", employee.Position ?? "");
                    command.Parameters.AddWithValue("@Salary", employee.Salary);
                    command.Parameters.AddWithValue("@HireDate", employee.HireDate);
                    
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM employees WHERE Id = @Id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}
