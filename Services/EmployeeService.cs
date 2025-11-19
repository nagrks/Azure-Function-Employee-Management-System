using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MyProjFolder.Models;

namespace MyProjFolder.Services
{
    public class az_employeeservice
    {
        private readonly string _connectionString;

        public az_employeeservice(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Employee>> GetAllaz_employeesAsync()
        {
            var az_employees = new List<Employee>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, Name, Email, Position, Salary, HireDate FROM az_employees";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            az_employees.Add(new Employee
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                Position = reader.GetString(3),
                                Salary = reader.GetDecimal(4),
                                HireDate = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }

            return az_employees;
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, Name, Email, Position, Salary, HireDate FROM az_employees WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Employee
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                Position = reader.GetString(3),
                                Salary = reader.GetDecimal(4),
                                HireDate = reader.GetDateTime(5)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<int> CreateEmployeeAsync(Employee employee)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"INSERT INTO az_employees (Name, Email, Position, Salary, HireDate) 
                                VALUES (@Name, @Email, @Position, @Salary, @HireDate);
                                SELECT CAST(SCOPE_IDENTITY() as int);";
                
                using (var command = new SqlCommand(query, connection))
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
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"UPDATE az_employees 
                               SET Name = @Name, Email = @Email, Position = @Position, 
                                   Salary = @Salary, HireDate = @HireDate 
                               WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
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
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM az_employees WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}
