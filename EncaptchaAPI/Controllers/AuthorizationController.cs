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
            if (data.Title >= JobTitles.Admin)
                return BadRequest("Title isn't correct");
            var item = new User()
            {
                Email = data.Email,
                Password = data.Password,
                JobTitle = data.Title
            };
            await _context.Users.AddAsync(item);
            await _context.SaveChangesAsync();
            return Ok(CreateJwtToken(item, _settings));
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
            return Ok(CreateJwtToken(user, _settings));
        }


        public static string CreateJwtToken(User user, AuthorizationSettings settings)
        {
            var claims = new List<Claim> 
            { 
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.JobTitle.ToString())
            };

            var jwt = new JwtSecurityToken(
                    issuer: settings.Issures,
                    audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromHours(settings.ExpiresHours)),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private readonly AuthorizationSettings _settings;
        private readonly UserContext _context;
    }
    public record class LoginData(string Email, string Password);
    public record class RegisterData(string Email, string Password, JobTitles Title) : LoginData(Email, Password);
}
