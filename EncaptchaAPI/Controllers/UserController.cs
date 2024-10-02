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
        public async Task<IAsyncEnumerable<User>> GetUsers()
        {
            return _context.Users.AsAsyncEnumerable();
        }

        [Route("customer/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            return Ok(await _context.Users.FirstAsync(c => c.Id == id));
        }

        [Route("customers")]
        [HttpPost]
        public async Task<IActionResult> PostUser(UserData customer) 
        {
            var item = new User()
            {
                Email = customer.Email,
                Password = customer.Password,
                JobTitle = customer.Title
            };
            await _context.Users.AddAsync(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }


        [Route("customer/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstAsync(c => c.Id == id);
            //if (user.JobTitle >= JobTitles.Admin)
            //    return BadRequest("You don't have enough rights");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private readonly UserContext _context;
    }
    public record class UserData(string Email, string Password, JobTitles Title);
}
