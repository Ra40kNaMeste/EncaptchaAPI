using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EncaptchaAPI.Controllers
{
    /// <summary>
    /// Контроллер для регистрации и авторизации пользователей
    /// </summary>
    public class AuthorizationController : Controller
    {
        public AuthorizationController(UserContext context, AuthorizationSettings settings) 
        {
            _context = context; 
            _settings = settings;
        }

        /// <summary>
        /// Регистрация для обычных людей
        /// </summary>
        /// <param name="data">Параметры для регистрации</param>
        /// <returns>jwt-токен пользователя</returns>
        [Route("registration")]
        [HttpPost]
        public async Task<IActionResult> RegistrationUser(RegisterData data)
        {
            //Нельзя регистрироваться на высшие должности сайта
            if (data.Title >= Roles.Admin)
                return BadRequest("Title isn't correct");
            //Нельзя регистрироваться с одинаковым емайлом
            if (_context.Users.Any(i=>i.Email == data.Email))
                return BadRequest("Email isn't free. Change one, please");

            var item = new User()
            {
                Email = data.Email,
                Password = data.Password,
                Role = data.Title
            };
            await _context.Users.AddAsync(item);
            await _context.SaveChangesAsync();
            return Ok(CreateJwtToken(item));
        }

        /// <summary>
        /// Авторизация пользователя
        /// </summary>
        /// <param name="data">Параметры входа</param>
        /// <returns>jwt-токен пользователя</returns>
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginUser(LoginData data)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == data.Email);
            if (user == null)
                return BadRequest("User not found");
            if (user.Password != data.Password)
                return BadRequest("Password isn't correct");
            return Ok(CreateJwtToken(user));
        }

        /// <summary>
        /// бэкдор для создания аккаунтов администрации сайта. В качестве секретного ключа необходимо передать ключ из настроек сервера
        /// </summary>
        /// <param name="data">Параметры пользователя</param>
        /// <param name="secretKey">Секретный ключ из настроек</param>
        /// <returns>jwt-токен пользователя</returns>
        [Route("registration/super/secret/path")]
        [HttpPost]
        public async Task<IActionResult> SecretRegistrationUser(RegisterData data, string secretKey)
        {
            //Если не передан в качестве параметра ключ - отклоняем
            if (!(secretKey == _settings.KeyForAdminRegistration))
                return NotFound();
            //Если пользователь уже был добавлен - отклоняем
            if (_context.Users.Any(i => i.Email == data.Email))
                return NotFound();

            //Добавляем пользователя с любыми параметрами
            var item = new User()
            {
                Email = data.Email,
                Password = data.Password,
                Role = data.Title
            };
            await _context.Users.AddAsync(item);
            await _context.SaveChangesAsync();
            return Ok(CreateJwtToken(item));
        }

        /// <summary>
        /// Функция генерации jwt-токена для пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>jwt-токен</returns>
        private string CreateJwtToken(User user)
        {
            var claims = new List<Claim> 
            { 
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, Enum.GetName(user.Role))
            };

            var jwt = new JwtSecurityToken(
                    issuer: _settings.Issures,
                    audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromHours(_settings.ExpiresHours)),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private readonly AuthorizationSettings _settings;
        private readonly UserContext _context;
    }
    public record class LoginData(string Email, string Password);
    public record class RegisterData(string Email, string Password, Roles Title) : LoginData(Email, Password);
}
