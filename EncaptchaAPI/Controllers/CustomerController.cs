using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EncaptchaAPI.Controllers
{
    public class CustomerController : Controller
    {
        public CustomerController(UserContext context) 
        {
            _context = context;
        }

        [Route("customers")]
        [HttpGet]
        public async Task<IAsyncEnumerable<Customer>> GetCustomes()
        {
            return _context.Customers.AsAsyncEnumerable();
        }

        [Route("customer/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetCustomer(int id)
        {
            return Ok(await _context.Customers.FirstAsync(c => c.Id == id));
        }

        [Route("customers")]
        [HttpPost]
        public async Task<IActionResult> PostCustomer(CustomerData customer) 
        {
            var item = new Customer()
            {
                Email = customer.Email,
                Password = customer.Password
            };
            await _context.Customers.AddAsync(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }


        [Route("customer/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            _context.Customers.Remove(_context.Customers.First(c => c.Id == id));
            await _context.SaveChangesAsync();
            return Ok();
        }

        private readonly UserContext _context;
    }
    public record class CustomerData(string Email, string Password);
}
