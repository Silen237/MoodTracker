using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MoodTracker.Data;
using Microsoft.EntityFrameworkCore;
using MoodTracker.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoodTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context; // 讀取資料庫的 DbContext
        private readonly IConfiguration _config; // 讀取 appsettings.json 的設定值

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }     

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Username 和 Email 不可為空" });
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if(emailExists)
            {
                return BadRequest(new { message = "Email 已被註冊" });
            }

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password // 先存明文，之後做加密
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(
            new
            {
                message = "註冊成功",
                username = request.Username,
                email = request.Email
            }
            );
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 從 DB 查詢使用者
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 驗證帳號存在 + 密碼正確（目前明文比對）
            if (user == null || user.Password != request.Password)
            {
                return Unauthorized(new { message = "帳號或密碼錯誤" });
            }

            // 產生 JWT
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "登入成功",
                token = token
            });

        }

        // 產生 JWT Token
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expiresInDays = int.Parse(jwtSettings["ExpiresInDays"]!);

            // Claims：夾帶在 Token 裡的使用者資訊
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username)
            };

            // 使用對稱式加密演算法產生簽章
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiresInDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 放在同一個檔案底部，之後熟悉了可以移到 Models/ 資料夾
        public class RegisterRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
