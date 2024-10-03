using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace EncaptchaAPI.Controllers
{
    public class CaptchaForCustomersTaskController : Controller
    {
        public CaptchaForCustomersTaskController(UserContext context, PricesSettings prices) 
        { 
            _context = context;
            _prices = prices;
        }

        [Route("captures")]
        [Authorize(Roles = nameof(Roles.Admin))]
        [HttpGet]
        public async Task<IAsyncEnumerable<CaptchaTask>> GetCaptures()
        {
            return _context.Captures
                .Include(i=>i.Customer)
                .Include(i=>i.Employee)
                .AsAsyncEnumerable();
        }

        [Route("captcha/{id}")]
        [Authorize(Roles = nameof(Roles.Customer))]
        [HttpGet]
        public async Task<IActionResult> GetCaptcha(int id)
        {
            var user = GetAuthUser();
            if (user == null)
                return BadRequest("User not found");
            var captcha = await _context.Captures
                .Include(i=>i.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(captcha == null || captcha.Customer != user)
                return NotFound();
            if (captcha.Mode != TaskMode.Completed)
                return BadRequest("CAPTCHA_NOT_READY");

            return Ok(captcha);
        }

        [Route("captures")]
        [Authorize(Roles = nameof(Roles.Customer))]
        [HttpPost]
        public async Task<IActionResult> PostCaptcha(IFormFile file)
        {
            var user = GetAuthUser();
            if(user == null)
                return BadRequest("User not found");

            var bytes = new byte[file.Length];
            using var stream = file.OpenReadStream();
            var sInt = sizeof(int);
            for (int i = 0; i < sizeof(long) / sInt; i++)
                if (await stream.ReadAsync(bytes, i, (int)(file.Length << i * sInt)) == file.Length + i * sInt)
                    break;
            bytes = Convert.FromBase64String(Encoding.Default.GetString(bytes));
            var item = new CaptchaTask()
            {
                Customer = user,
                Captcha = bytes,
                Mode = TaskMode.Created
            };
            await _context.Captures.AddAsync(item);
            await _context.SaveChangesAsync();
            return Ok(item.Id);
        }


        [Route("captcha/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCaptcha(int id)
        {
            var user = GetAuthUser();
            if(user == null)
                return BadRequest("User not found");

            var capture = await _context.Captures.FirstOrDefaultAsync(c => c.Id == id);
            if(capture == null || capture.Customer != user)
                return NotFound();

            _context.Captures.Remove(capture);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private User? GetAuthUser()
        {
            string? email = ControllerContext.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
                return null;
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        private readonly PricesSettings _prices;
        private readonly UserContext _context;
    }
}
