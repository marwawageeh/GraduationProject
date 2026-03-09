using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		private readonly EmailService _email;

		public AuthController(AppDBContext db, IConfiguration config)
		{
			_db = db;
			_config = config;
			_email = new EmailService(); 
		}


		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterDto dto)
		{
			if (_db.users.Any(u => u.Email == dto.Email))
				return BadRequest("Email already exists");

			var code = new Random().Next(100000, 999999).ToString();

			var pending = new PendingUser
			{
				Name = dto.Name,
				Email = dto.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
				Phone = dto.Phone,
				Code = code,
				ExpireAt = DateTime.Now.AddMinutes(10)
			};

			_db.PendingUsers.Add(pending);
			await _db.SaveChangesAsync();

			_email.SendEmail(dto.Email,
				"OTP Code",
				$"Your verification code is {code}");

			return Ok("OTP Sent");
		}



		[HttpPost("verify-register")]
		public async Task<IActionResult> VerifyRegister(string email, string code)
		{
			var pending = await _db.PendingUsers
				.FirstOrDefaultAsync(x => x.Email == email && x.Code == code);

			if (pending == null || pending.ExpireAt < DateTime.Now)
				return BadRequest("Invalid or expired code");

			var user = new User
			{
				Name = pending.Name,
				Email = pending.Email,
				PasswordHash = pending.PasswordHash,
				Phone = pending.Phone,
				Role = "Patient"
			};


			_db.users.Add(user);
			_db.PendingUsers.Remove(pending);
			await _db.SaveChangesAsync();

			_db.NotificationSettings.Add(new NotificationSettings
			{
				UserId = user.UserId
			});

			await _db.SaveChangesAsync();

			return Ok("Account created successfully");
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


		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
		{
			var email = dto.Email;

			var user = await _db.users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
				return NotFound("Email not found");

			var code = new Random().Next(1000, 9999).ToString();

			_db.PasswordResetCodes.Add(new PasswordResetCode
			{
				Email = email,
				Code = code,
				ExpireAt = DateTime.Now.AddMinutes(10)
			});

			await _db.SaveChangesAsync();

			// هنا تحطى خدمة ارسال ايميل
			_email.SendEmail(email,
	           "Verification Code",
	           $"Your verification code is {code}");


			return Ok("Code sent");
		}


		[HttpPost("verify-code")]
		public async Task<IActionResult> VerifyCode(string email, string code)
		{
			var record = await _db.PasswordResetCodes
				.FirstOrDefaultAsync(x => x.Email == email && x.Code == code);

			if (record == null || record.ExpireAt < DateTime.Now)
				return BadRequest("Invalid or expired code");

			return Ok("Verified");
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(string email, string newPassword)
		{
			var user = await _db.users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
				return NotFound();

			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

			await _db.SaveChangesAsync();

			return Ok("Password updated");
		}


	}
}
