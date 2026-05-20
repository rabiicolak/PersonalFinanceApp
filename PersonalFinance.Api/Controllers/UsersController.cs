using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Api.Data;
using PersonalFinance.Api.Models;

namespace PersonalFinance.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == user.Email);

            if (existingUser != null)
            {
                return BadRequest("Bu email zaten kayıtlı.");
            }

            user.CreatedDate = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Kayıt başarılı.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    x.Password == password);

            if (user == null)
            {
                return BadRequest("Email veya şifre hatalı.");
            }

            return Ok(user);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(
            int userId,
            string oldPassword,
            string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            if (user.Password != oldPassword)
            {
                return BadRequest("Eski şifre yanlış.");
            }

            user.Password = newPassword;
            user.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("Şifre güncellendi.");
        }
    }
}