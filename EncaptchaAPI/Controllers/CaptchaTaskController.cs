using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EncaptchaAPI.Controllers
{
    public class CaptchaTaskController : Controller
    {
        public CaptchaTaskController(UserContext context) { _context = context; }

        [Route("captures")]
        [HttpGet]
        public async Task<IAsyncEnumerable<CaptchaTask>> GetCaptures()
        {
            return _context.Captures.AsAsyncEnumerable();
        }

        [Route("captcha/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetCaptcha(int id)
        {
            var captcha = await _context.Captures.FirstOrDefaultAsync(x => x.Id == id);
            if(captcha == null)
                return NotFound();
            if (captcha.Mode != TaskMode.Completed)
                return BadRequest("CAPTCHA_NOT_READY");

            return Ok(captcha);
        }

        [Route("captures")]
        [HttpPost]
        public async Task<IActionResult> PostEmployee(IFormFile file)
        {
            var bytes = new byte[file.Length];
            using var stream = file.OpenReadStream();
            var sInt = sizeof(int);
            for (int i = 0; i < sizeof(long) / sInt; i++)
                if (await stream.ReadAsync(bytes, i, (int)(file.Length << i * sInt)) == file.Length + i * sInt)
                    break;
            bytes = Convert.FromBase64String(Encoding.Default.GetString(bytes));
            var item = new CaptchaTask()
            {
                Captcha = bytes,
                Mode = TaskMode.Created
            };
            await _context.Captures.AddAsync(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }


        [Route("captcha/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            _context.Employees.Remove(_context.Employees.First(c => c.Id == id));
            await _context.SaveChangesAsync();
            return Ok();
        }

        private readonly UserContext _context;
    }
}
