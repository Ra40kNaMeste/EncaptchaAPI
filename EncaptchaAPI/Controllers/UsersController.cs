using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EncaptchaAPI.Controllers
{
    public class UsersController : Controller
    {
        public UsersController(UserContext context) 
        {
            _context = context;
        }

        [Route("users")]
        [HttpGet]
        public async Task<IAsyncEnumerable<UserView>> GetUsers()
        {
            return _context.Users.Select(i=>i as UserView).AsAsyncEnumerable();
        }

        [Route("user/{id}")]
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

        [Route("user/{id}")]
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> ChangeRole(int id, Roles role)
        {
            var target = await _context.Users.FirstOrDefaultAsync(c => c.Id == id);

            string? email = ControllerContext.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(i => i.Email == email);

            if (user == null)
                return BadRequest("Authorization error");
            if (target == null)
                return BadRequest("User not found");

            //Условие изменения собственной роли обычным смертным
            var CanSelfCondition = () => user == target && (role == Roles.Employee || role == Roles.Customer);

            //Условие изменения роли для адимнистрации
            var CanOtherCondition = () => user.Role >= Roles.Admin && user.Role > target.Role && role < user.Role;

            if (!CanSelfCondition() || ! CanOtherCondition())
                return BadRequest("You don't have enough rights");

            target.Role = role;
            await _context.SaveChangesAsync();
            return Ok("Role was change succesfull");
        }

        [Route("user/{id}")]
        [Authorize]
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
