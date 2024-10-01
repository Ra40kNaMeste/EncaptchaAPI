using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EncaptchaAPI;

namespace EncaptchaAPI.Controllers
{
    public class EmployeesController : Controller
    {

        public EmployeesController(UserContext context)
        {
            _context = context;
        }

        [Route("employees")]
        [HttpGet]
        public async Task<IAsyncEnumerable<Employee>> GetEmployees()
        {
            return _context.Employees.AsAsyncEnumerable();
        }

        [Route("employee/{id}")]
        [HttpGet]
        public async Task<IActionResult> Getemployee(int id)
        {
            return Ok(await _context.Employees.FirstAsync(c => c.Id == id));
        }

        [Route("employees")]
        [HttpPost]
        public async Task<IActionResult> PostEmployee(EmployeeData data)
        {
            await _context.Employees.AddAsync(new()
            {
                Email = data.Email,
                Password = data.Password,
                Cache = 0
            });
            await _context.SaveChangesAsync();
            return Ok(_context.Employees.Where(i => i.Email == data.Email));
        }


        [Route("employee/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            _context.Employees.Remove(_context.Employees.First(c => c.Id == id));
            await _context.SaveChangesAsync();
            return Ok();
        }

        private readonly UserContext _context;
    }
    public record class EmployeeData(string Email, string Password);
}
