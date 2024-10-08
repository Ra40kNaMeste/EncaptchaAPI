using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EncaptchaAPI.Controllers
{
    public class UserController : Controller
    {
        public UserController(UserContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получение информации о авторизированном пользователе
        /// </summary>
        /// <returns></returns>
        [Route("user")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var user = await GetAuthUser();
            if (user == null)
                return StatusCode(StatusCodes.Status401Unauthorized);
            return Ok(new UserView(user));
        }

        /// <summary>
        /// Удаление авторизированного пользователя
        /// </summary>
        /// <returns></returns>
        [Route("user")]
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            var user = await GetAuthUser();
            if (user == null)
                return StatusCode(StatusCodes.Status401Unauthorized);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Изменение роли авторизированного пользователя
        /// </summary>
        /// <param name="role">Новая роль</param>
        /// <returns></returns>
        [Route("user")]
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> ChangeRole(Roles role)
        {
            var target = await GetAuthUser();

            if (target == null)
                return BadRequest("User not found");

            //Условие изменения собственной роли обычным смертным
            var CanSelfCondition = () => role == Roles.Employee || role == Roles.Customer;

            //Условие изменения роли для адимнистрации
            var CanOtherCondition = () => target.Role >= Roles.Admin && role <= target.Role;

            if (CanSelfCondition() || CanOtherCondition())
            {
                target.Role = role;
                await _context.SaveChangesAsync();
                return Ok("Role was change succesfull");
            }
            return BadRequest("You don't have enough rights");
        }
        private async Task<User?> GetAuthUser()
        {
            string? email = ControllerContext.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            return email == null? null: await _context.Users.FirstAsync(i=>i.Email == email);
        }

        private readonly UserContext _context;
    }
}
