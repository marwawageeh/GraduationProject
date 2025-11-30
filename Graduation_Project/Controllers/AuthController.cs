using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly AppDBContext _db;
		private readonly IConfiguration _config;

		public AuthController(AppDBContext db, IConfiguration config)
		{
			_db = db;
			_config = config;
		}


		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterDto dto)
		{
			if (_db.users.Any(u => u.Email == dto.Email)) return BadRequest("Email exists");
			var user = new User
			{
				Name = dto.Name,
				Email = dto.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
				Role = "Patient",
				Phone = dto.Phone
			};
			_db.users.Add(user);
			await _db.SaveChangesAsync();
			return Ok();
		}

		[HttpPost("login")]
		public IActionResult Login(LoginDto dto)
		{
			var user = _db.users.SingleOrDefault(u => u.Email == dto.Email);
			if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
			{
				return Unauthorized();
			}

			var token = GenerateJwtToken(user);
			return Ok(new { token, userId = user.UserId, name = user.Name, role = user.Role });
		}

		private string GenerateJwtToken(User user)
		{
			var claims = new[] {
			new Claim(JwtRegisteredClaimNames.Sub, user.Email),
			new Claim("uid", user.UserId.ToString()),
			new Claim(ClaimTypes.Role, user.Role)
		};
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims, expires: DateTime.Now.AddDays(7), signingCredentials: creds);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
