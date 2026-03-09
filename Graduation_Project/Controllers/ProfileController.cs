using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class ProfileController : ControllerBase
	{
		private readonly AppDBContext _context;
		private readonly IWebHostEnvironment _env;

		public ProfileController(AppDBContext DB, IWebHostEnvironment env)
        {
			_context = DB;
			_env = env;

		}


		// ================== My Rentals ==================
		//[HttpGet("rentals/{id}")]
		[HttpGet("{id}")]
		public async Task<IActionResult> GetMyRentals(int id)
		{
			var rentals = await _context.Rental
				.Include(r => r.Equipment)
				.Where(r => r.UserId == id)
				.Select(r => new
				{
					r.RentalId,
					EquipmentiId=r.EquipmentId,
					EquipmentName = r.Equipment.Name,
					ImageUrl = r.Equipment.ImageUrl,
					r.StartDate,
					r.EndDate,
					r.TotalPrice
				})
				.ToListAsync();   // 👈 هنا البيانات طلعت من DB خلاص

			if (!rentals.Any())
				return NotFound("No rentals found");

			// 👇 نحسب Status بعد ما البيانات خرجت من SQL
			var result = rentals.Select(r => new
			{
				r.RentalId,
				r.EquipmentiId,
				r.EquipmentName,
				r.ImageUrl,
				r.StartDate,
				r.EndDate,
				r.TotalPrice,
				Status = GetRentalStatus(r.StartDate, r.EndDate)
			});

			return Ok(result);
		}


		// ================== Search in My Rentals ==================
		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] int userId, [FromQuery] string? name)
		{
			var rentals = await _context.Rental
				.Include(r => r.Equipment)
				.Where(r => r.UserId == userId)
				.ToListAsync();

			if (!string.IsNullOrEmpty(name))
				rentals = rentals.Where(r => r.Equipment.Name.Contains(name)).ToList();

			var result = rentals.Select(r => new
			{
				r.RentalId,
				EquipmentName = r.Equipment.Name,
				ImageUrl = r.Equipment.ImageUrl,
				r.StartDate,
				r.EndDate,
				r.TotalPrice,
				Status = GetRentalStatus(r.StartDate, r.EndDate)
			});

			return Ok(result);
		}


		// ================== Status Logic ==================
		private string GetRentalStatus(DateTime start, DateTime end)
		{
			var today = DateTime.Now.Date;

			if (today < start)
				return "Pending";     // لم يبدأ
			if (today >= start && today <= end)
				return "Active";      // شغال
			return "Completed";       // انتهى
		}


		[HttpGet]
		public async Task<IActionResult> GetProfile()
		{
			var userId = int.Parse(User.FindFirst("uid").Value);

			var user = await _context.users.FindAsync(userId);

			if (user == null)
				return NotFound();

			var result = new ProfileResponseDto
			{
				UserId = user.UserId,
				Name = user.Name,
				DateOfBirth = user.DateOfBirth,
				Gender = user.Gender,
				Phone = user.Phone,
				Email = user.Email,
				ImageUrl = user.ImageUrl
			};

			return Ok(result);
		}




		//[HttpGet("{id}")]
		//public async Task<IActionResult> GetProfile(int id)
		//{
		//	var user = await _context.users.FindAsync(id);


		//	if (user == null)
		//		return NotFound();

		//	var result = new ProfileResponseDto
		//	{
		//		UserId = user.UserId,
		//		Name = user.Name,
		//		DateOfBirth = user.DateOfBirth,
		//		Gender = user.Gender,
		//		Phone = user.Phone,
		//		Email = user.Email,
		//		ImageUrl = user.ImageUrl
		//	};

		//	return Ok(result);
		//}


		//// 🟢 Update Profile
		//[HttpPut("{id}")]
		//public async Task<IActionResult> UpdateProfile(int id, UpdateProfileDto dto)
		//{
		//	var user = await _context.users.FindAsync(id);

		//	if (user == null)
		//		return NotFound();

		//	user.Email = dto.Email;
		//	user.Name = dto.Name;
		//	user.DateOfBirth = dto.DateOfBirth;
		//	user.Gender = dto.Gender;
		//	user.Phone = dto.Phone;

		//	await _context.SaveChangesAsync();

		//	return Ok(new { message = "Profile updated successfully" });
		//}

		[HttpPut]
		public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
		{
			var userId = int.Parse(User.FindFirst("uid").Value);

			var user = await _context.users.FindAsync(userId);

			if (user == null)
				return NotFound();

			user.Email = dto.Email;
			user.Name = dto.Name;
			user.DateOfBirth = dto.DateOfBirth;
			user.Gender = dto.Gender;
			user.Phone = dto.Phone;

			await _context.SaveChangesAsync();

			return Ok(new { message = "Profile updated successfully" });
		}



		[HttpPost("upload-image")]
		public async Task<IActionResult> UploadProfileImage(IFormFile file)
		{
			var userId = int.Parse(User.FindFirst("uid").Value);

			var user = await _context.users.FindAsync(userId);

			if (user == null)
				return NotFound("User not found");

			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded");

			var folderPath = Path.Combine(_env.WebRootPath, "images", "users");

			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
			var filePath = Path.Combine(folderPath, fileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			user.ImageUrl = "/images/users/" + fileName;

			await _context.SaveChangesAsync();

			return Ok(new { imageUrl = user.ImageUrl });
		}


	}
}
