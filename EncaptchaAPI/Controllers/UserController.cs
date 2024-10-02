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
}
