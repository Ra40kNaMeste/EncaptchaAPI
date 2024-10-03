using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;

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
        public async Task<IAsyncEnumerable<UserView>> GetUsers()
        {
            return _context.Users.Select(i=>i as UserView).AsAsyncEnumerable();
        }

        [Route("customer/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            var person = await _context.Users
                .Include(i=>i.CustomeredTasks)
                    .ThenInclude(i=>i.Employee)
                .Include(i=>i.CompletedTasks)
                    .ThenInclude(i=>i.Customer)
                .FirstAsync(c => c.Id == id);

            if(CanAuthUser(person))
                return Ok(person);

            return Ok(new UserView(person));
        }

        [Route("customer/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var person = await _context.Users.FirstAsync(c => c.Id == id);
            if (CanAuthUser(person))
            {
                _context.Users.Remove(person);
                await _context.SaveChangesAsync();
                return Ok(person);
            }         
            return BadRequest("You don't have enough rights");
        }

        private bool CanAuthUser(User target)
        {
            string? email = ControllerContext.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            return email != null && target != null && (target.Email == email || target.Role >= Roles.Admin);
        }

        private readonly UserContext _context;
    }
}
