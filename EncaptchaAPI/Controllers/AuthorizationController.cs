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
    public class AuthorizationController : Controller
    {
        public AuthorizationController(UserContext context, AuthorizationSettings settings) 
        {
            _context = context; 
            _settings = settings;
        }

        [Route("registration")]
        [HttpPost]
        public async Task<IActionResult> RegistrationUser(RegisterData data)
        {
            if (data.Title >= Roles.Admin)
                return BadRequest("Title isn't correct");
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


        private string CreateJwtToken(User user)
        {
            var claims = new List<Claim> 
            { 
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, Enum.GetName(user.Role))
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
