using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TheDugout.Data;
using TheDugout.Models;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(DugoutDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            using var sha = SHA256.Create();
            var hash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return Unauthorized("Невалидни данни");

            using var sha = SHA256.Create();
            var hash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

            if (user.PasswordHash != hash) return Unauthorized("Грешна парола");

            var token = GenerateJwtToken(user);
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Промени на true за HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { Message = "Успешен вход", Token = token });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (!Request.Cookies.TryGetValue("jwt", out var token))
                return Unauthorized();

            var claimsPrincipal = ValidateToken(token);
            if (claimsPrincipal == null) return Unauthorized();

            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Unauthorized();

            return Ok(new { user.Id, user.Username, user.Email });
        }

        // Logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged out successfully" });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
    };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            try
            {
                return tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true
                }, out _);
            }
            catch
            {
                return null;
            }
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}


