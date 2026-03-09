using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduation_Project.Controllers
{
	[Authorize(Roles = "EquipmentOwner")]
	[Route("api/[controller]")]
	[ApiController]
	public class EquipmentOwnerController : ControllerBase
	{
		private readonly AppDBContext _context;

        public EquipmentOwnerController(AppDBContext context)
        {
			_context = context;
		}

		[HttpGet("EquipmentOwner/dashboard")]
		public async Task<IActionResult> GetOwnerDashboard()
		{
			var claim = User.FindFirst("uid");

			if (claim == null)
				return Unauthorized("User claim missing");

			var ownerId = int.Parse(claim.Value);

			var today = DateTime.Today;

			// Total Devices
			var totalDevices = await _context.Equipments
				.CountAsync(e => e.OwnerId == ownerId);

			// All Rentals for Owner Devices
			var rentals = _context.Rental
				.Include(r => r.Equipment)
				.Where(r => r.Equipment.OwnerId == ownerId);

			// Today Rentals
			var todayRentals = await rentals
				.CountAsync(r => r.StartDate.Date == today);

			// Pending
			var pending = await rentals
				.CountAsync(r => r.Status == "PendingPayment");

			// Rating (Average of equipment reviews)
			var rating = await _context.Reviews
				.Where(r => _context.Equipments
					.Any(e => e.EquipmentId == r.EquipmentId && e.OwnerId == ownerId))
				.AverageAsync(r => (double?)r.Rating) ?? 0;

			int diff = (7 + (today.DayOfWeek - DayOfWeek.Saturday)) % 7;
			var startOfWeek = today.AddDays(-1 * diff).Date;
			var endOfWeek = startOfWeek.AddDays(7);

			var weeklyData = await _context.Rental
				.Include(r => r.Equipment)
				.Where(r => r.Equipment.OwnerId == ownerId)
				.Where(r => r.StartDate >= startOfWeek
						 && r.StartDate < endOfWeek)
				.ToListAsync();   // 👈 هنا جبنا البيانات الأول

			var weeklyRental = weeklyData
				.GroupBy(r => r.StartDate.DayOfWeek)  // 👈 هنا بقى في الذاكرة
				.Select(g => new WeeklyRentalDto
				{
					Day = g.Key.ToString(),
					Count = g.Count()
				})
				.OrderBy(x => x.Day)
				.ToList();

			// Rental Types (حسب مدة الإيجار)
			var rentalTypes = await rentals
				.GroupBy(r => EF.Functions.DateDiffDay(r.StartDate, r.EndDate) > 6
					? "Weekly Rental"
					: "Daily Rental")
				.Select(g => new RentalTypeDto
				{
					Type = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			var result = new OwnerDashboardDto
			{
				TotalDevices = totalDevices,
				TodayRentals = todayRentals,
				PendingRentals = pending,
				Rating = Math.Round(rating, 1),
				WeeklyRentals = weeklyRental,
				RentalTypes = rentalTypes
			};

			return Ok(result);
		}

		[HttpGet("devices")]
		public async Task<IActionResult> GetOwnerDevices()
		{
			var claim = User.FindFirst("uid");

			if (claim == null)
				return Unauthorized("User claim missing");

			var ownerId = int.Parse(claim.Value);

			var today = DateTime.Today;

			var devices = await _context.Equipments
				.Where(e => e.OwnerId == ownerId)
				.Select(e => new OwnerDeviceDto
				{
					EquipmentId = e.EquipmentId,
					Name = e.Name,
					Description = e.Description,
					PricePerDay = e.PricePerDay,

					Rating = _context.Reviews
						.Where(r => r.EquipmentId == e.EquipmentId)
						.Average(r => (double?)r.Rating) ?? 0,

					Status =
						e.IsUnderMaintenance ? "Maintenance" :
						_context.Rental.Any(r =>
							r.EquipmentId == e.EquipmentId &&
							r.StartDate <= today &&
							r.EndDate >= today)
						? "Booked"
						: "Avaliable"
				})
				.ToListAsync();

			return Ok(devices);
		}

		[HttpPost("add-device")]
		public async Task<IActionResult> AddDevice([FromForm] AddEquipmentDto model)
		{
			var claim = User.FindFirst("uid");

			if (claim == null)
				return Unauthorized("User claim missing");

			var ownerId = int.Parse(claim.Value);

			if (model == null)
				return BadRequest("Invalid Data");

			string imagePath = null;

			// رفع الصورة
			if (model.Image != null)
			{
				var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
				var filePath = Path.Combine(folderPath, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await model.Image.CopyToAsync(stream);
				}

				imagePath = "/images/" + fileName;
			}

			var equipment = new Equipment
			{
				Name = model.Name,
				Description = model.Description,
				PricePerDay = model.PricePerDay,
				OwnerId = ownerId,
				Availability = true,
				IsUnderMaintenance = false,
				ImageUrl = imagePath
			};

			_context.Equipments.Add(equipment);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Device Added Successfully" });
		}


		[HttpGet("bookings")]
		public async Task<IActionResult> GetOwnerBookings(string? status, string? search)
		{
			var claim = User.FindFirst("uid");

			if (claim == null)
				return Unauthorized("User claim missing");

			var ownerId = int.Parse(claim.Value);

			var query = _context.Rental
				.Include(r => r.Equipment)
				.Include(r => r.User)   // العميل
				.Where(r => r.Equipment.OwnerId == ownerId);

			// فلترة بالحالة
			if (!string.IsNullOrEmpty(status))
			{
				query = query.Where(r => r.Status == status);
			}

			// البحث بالاسم أو رقم الموبايل
			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(r =>
					r.User.Name.Contains(search) ||
					r.User.Phone.Contains(search));
			}

			var bookings = await query
				.OrderByDescending(r => r.StartDate)
				.Select(r => new OwnerBookingDto
				{
					RentalId = r.RentalId,
					CustomerName = r.User.Name,
					CustomerPhone = r.User.Phone,
					DeviceName = r.Equipment.Name,
					Date = r.StartDate.Date,
					Time = r.StartDate.ToString("hh:mm tt"),
					Type = EF.Functions.DateDiffDay(r.StartDate, r.EndDate) > 6
						? "Weekly Rental"
						: "Daily Rental",
					Status = r.Status
				})
				.ToListAsync();

			return Ok(bookings);
		}

		[HttpGet("booking/{rentalId}")]
		public async Task<IActionResult> GetBookingDetails(int rentalId)
		{
			var booking = await _context.Rental
				.Include(r => r.User)
				.Include(r => r.Equipment)
				.Where(r => r.RentalId == rentalId)
				.Select(r => new
				{
					r.RentalId,
					Customer = r.User.Name,
					Phone = r.User.Phone,
					Device = r.Equipment.Name,
					r.StartDate,
					r.EndDate,
					r.Status,
					r.TotalPrice
				})
				.FirstOrDefaultAsync();

			if (booking == null)
				return NotFound();

			return Ok(booking);
		}

	}
}
