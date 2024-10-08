using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace EncaptchaAPI.Controllers
{
    public class CaptchaForEmployeesController : Controller
    {
        public CaptchaForEmployeesController(UserContext context, PricesSettings prices)
        {
            _context = context;
            _prices = prices;
        }

        object locker = new();

        /// <summary>
        /// Выдаёт самую древнюю задачу заказчику и закреляет её под ним
        /// </summary>
        /// <returns>Id задачи</returns>
        [Route("emp/captcha")]
        [Authorize(Roles = nameof(Roles.Employee))]
        [HttpGet]
        public async Task<IActionResult> GetFreeCaptcha()
        {
            var user = GetAuthUser();
            if (user == null)
                return BadRequest("User not found");

            CaptchaTask? captcha;
            lock (locker)
            {
                captcha = _context.Captures
                .Reverse()
                .FirstOrDefault(x => x.Mode == TaskMode.Created);

                if (captcha == null)
                    return NotFound();
                captcha.Mode = TaskMode.AtWork;
                captcha.Working = DateTime.Now;
                captcha.Employee = user;
                _context.SaveChanges();
            }
            return Ok(captcha.Id);
        }

        /// <summary>
        /// Выдаёт изображение кптчи по Id задачи
        /// </summary>
        /// <param name="id">Id капчи</param>
        /// <returns>Изображение задачи</returns>
        [Route("emp/captcha/{id}")]
        [Authorize(Roles = nameof(Roles.Employee))]
        [HttpGet]
        public async Task<IActionResult> GetCaptcha(int id)
        {
            var user = GetAuthUser();
            if (user == null)
                return BadRequest("User not found");

            var captcha = await _context.Captures
                .FirstOrDefaultAsync(x => x.Id == id);

            if (captcha == null || captcha.Employee != user)
                return NotFound();

            return File(captcha.Captcha, "image/jpeg");
        }

        /// <summary>
        /// Добавить результат капчи
        /// </summary>
        /// <param name="result">результат</param>
        /// <param name="id">Id капчи</param>
        /// <returns></returns>
        [Route("emp/captcha/{id}")]
        [Authorize(Roles = nameof(Roles.Employee))]
        [HttpPost]
        public async Task<IActionResult> PostCaptcha(string result, int id)
        {
            var user = GetAuthUser();
            if (user == null)
                return BadRequest("User not found");

            var captcha = await _context.Captures
                .FirstOrDefaultAsync(x => x.Id == id);

            if (captcha == null || captcha.Employee != user)
                return NotFound();

            captcha.Solution = result;
            captcha.Mode = TaskMode.Completed;
            captcha.Worked = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Отказ от выполнения капчи
        /// </summary>
        /// <param name="id">Id капчи</param>
        /// <returns></returns>
        [Route("emp/captcha/{id}")]
        [Authorize(Roles = nameof(Roles.Employee))]
        [HttpDelete]
        public async Task<IActionResult> DeleteCaptcha(int id)
        {
            var user = GetAuthUser();
            if (user == null)
                return BadRequest("User not found");

            var capture = await _context.Captures.FirstOrDefaultAsync(c => c.Id == id);
            if (capture == null || capture.Employee != user)
                return NotFound();

            capture.Mode = TaskMode.Created;
            capture.Employee = null;
            capture.Solution = "";
            capture.Working = null;
            capture.Worked = null;

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
