using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MyProjFolder.Models;
using MyProjFolder.Services;

namespace MyProjFolder
{
    public static class MyHttpTrigger
    {
        private static readonly string 
            // ConnectionString = "Server=localhost;Port=3306;Database=ems;Uid=root;Pwd=Amer1can;"; 
            ConnectionString = "Server=172.29.11.239;Database=ems;Uid=sa;Pwd=Amer1can;";
        private static readonly az_employeeservice az_employeeservice = new az_employeeservice(ConnectionString);

        [FunctionName("MyHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete", Route = "employee/{id?}")] HttpRequest req, 
            string id,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                // GET all az_employees
                if (req.Method == "GET" && string.IsNullOrEmpty(id))
                {
                    var az_employees = await az_employeeservice.GetAllaz_employeesAsync();
                    return new OkObjectResult(az_employees);
                }

                // GET employee by ID
                if (req.Method == "GET" && !string.IsNullOrEmpty(id) && int.TryParse(id, out int employeeId))
                {
                    var employee = await az_employeeservice.GetEmployeeByIdAsync(employeeId);
                    if (employee != null)
                    {
                        return new OkObjectResult(employee);
                    }
                    return new NotFoundObjectResult("Employee not found");
                }

                // POST - Create new employee
                if (req.Method == "POST")
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var employee = JsonConvert.DeserializeObject<Employee>(requestBody);

                    if (employee == null || string.IsNullOrEmpty(employee.Name))
                    {
                        return new BadRequestObjectResult("Employee name is required");
                    }

                    int newId = await az_employeeservice.CreateEmployeeAsync(employee);
                    employee.Id = newId;
                    return new CreatedResult($"employee/{newId}", employee);
                }

                // PUT - Update employee
                if (req.Method == "PUT" && !string.IsNullOrEmpty(id) && int.TryParse(id, out int updateId))
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var employee = JsonConvert.DeserializeObject<Employee>(requestBody);

                    if (employee == null || string.IsNullOrEmpty(employee.Name))
                    {
                        return new BadRequestObjectResult("Employee name is required");
                    }

                    employee.Id = updateId;
                    bool updated = await az_employeeservice.UpdateEmployeeAsync(employee);
                    
                    if (updated)
                    {
                        return new OkObjectResult("Employee updated successfully");
                    }
                    return new NotFoundObjectResult("Employee not found");
                }

                // DELETE - Delete employee
                if (req.Method == "DELETE" && !string.IsNullOrEmpty(id) && int.TryParse(id, out int deleteId))
                {
                    bool deleted = await az_employeeservice.DeleteEmployeeAsync(deleteId);
                    
                    if (deleted)
                    {
                        return new OkObjectResult("Employee deleted successfully");
                    }
                    return new NotFoundObjectResult("Employee not found");
                }

                return new BadRequestObjectResult("Invalid request");
            }
            catch (Exception ex)
            {
                log.LogError($"Error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
