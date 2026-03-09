using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
	[Authorize(Roles = "Admin")]
	[Route("api/[controller]")]
	[ApiController]
	public class AdminController : ControllerBase
	{
		private readonly AppDBContext _context;

        public AdminController(AppDBContext context)
        {
			_context = context;

		}

		[HttpGet("dashboard")]
		public async Task<IActionResult> GetDashboard()
		{
			var dashboard = new AdminDashboardDto();

			// ===== Cards =====
			dashboard.TotalUsers = await _context.users.CountAsync();
			dashboard.TotalDoctors = await _context.Doctors.CountAsync();
			dashboard.TotalBookings = await _context.Booking.CountAsync();
			dashboard.TotalRental = await _context.Rental.CountAsync();
			dashboard.TotalHospitals = await _context.Hospitals.CountAsync();
			dashboard.TotalEquipments = await _context.Equipments.CountAsync();

			var bookingRevenue = await _context.Booking
	            .Where(b => b.Status == "Confirmed" || b.Status == "Completed")
	            .SumAsync(b => (decimal?)b.Price) ?? 0;

			//var rentalRevenue = await _context.Rental
	  //          .Where(r => r.Status == "Confirmed" || r.Status == "Completed")
	  //          .SumAsync(r => (decimal?)r.TotalPrice) ?? 0;

			var rentalRevenue = await _context.Rental
	            .Include(r => r.Equipment)
	            .Where(r => r.Status == "Confirmed" || r.Status == "Booked")
	            .SumAsync(r =>
		            (decimal?)(
			        (EF.Functions.DateDiffDay(r.StartDate, r.EndDate) + 1)
			        * r.Equipment.PricePerDay
		          )
	             ) ?? 0;

			dashboard.TotalRevenue = bookingRevenue + rentalRevenue;

			var monthlyData = await _context.Booking
	              .GroupBy(b => b.BookingDate.Month)
	              .Select(g => new { Month = g.Key, Count = g.Count() })
	              .ToListAsync();

			dashboard.MonthlyBookings = Enumerable.Range(1, 12)
				.Select(month =>
					monthlyData.FirstOrDefault(m => m.Month == month)?.Count ?? 0
				)
				.ToList();

			var monthlyRentalsData = await _context.Rental
	             .GroupBy(r => r.StartDate.Month)
	             .Select(g => new { Month = g.Key, Count = g.Count() })
	             .ToListAsync();

			dashboard.MonthlyRentals = Enumerable.Range(1, 12)
				.Select(month =>
					monthlyRentalsData.FirstOrDefault(m => m.Month == month)?.Count ?? 0
				)
				.ToList();


			dashboard.MonthNames = Enumerable.Range(1, 12)
	              .Select(m => new DateTime(2026, m, 1).ToString("MMM"))
	              .ToList();

			// ===== Weekly Bookings =====
			var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

			dashboard.WeeklyBookings = Enumerable.Range(0, 7)
				.Select(d =>
				{
					var date = startOfWeek.AddDays(d);
					return _context.Booking
						.Count(b => b.BookingDate.Date == date.Date);
				}).ToList();


			dashboard.WeeklyDays = new List<string>
		    {
			"Sun","Mon","Tue","Wed","Thu","Fri","Sat"
		    };

			var bookingTypesData = await _context.Booking
	           .GroupBy(b => b.BookingTypes)
	           .Select(g => new
	           {
		          Type = g.Key,
		          Count = g.Count()
	           })
	           .ToListAsync();

			dashboard.BookingTypesNames = bookingTypesData
				.Select(b => b.Type)
				.ToList();

			dashboard.BookingTypesCount = bookingTypesData
				.Select(b => b.Count)
				.ToList();

			return Ok(dashboard);
		}

		[HttpGet("doctors")]
		public async Task<IActionResult> GetDoctors()
		{
			var doctors = await _context.Doctors
				.Include(d => d.Department)
				    .ThenInclude(d => d.Hospital)
				.Select(d => new
				{
					d.DoctorId,
					d.Name,
					d.Specialization,
					d.ExperienceYears,
					d.Rating,
					d.StartTime,
					d.EndTime,
					d.ConsultationPrice,
					d.ImageURL,
					Department = d.Department.Name,
					Hospital = d.Department.Hospital.Name
				})
				.ToListAsync();

			return Ok(doctors);
		}



		[HttpPost("add-doctor")]
		public async Task<IActionResult> AddDoctor([FromForm] AddDoctorDto dto)
		{
			if (!TimeSpan.TryParse(dto.StartTime, out var startTime))
				return BadRequest("Invalid Start Time format. Use HH:mm");

			if (!TimeSpan.TryParse(dto.EndTime, out var endTime))
				return BadRequest("Invalid End Time format. Use HH:mm");

			var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

			string imagePath = null;

			if (dto.Image != null)
			{
				var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
				var filePath = Path.Combine(folderPath, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await dto.Image.CopyToAsync(stream);
				}

				imagePath = "/images/" + fileName;
			}

			var user = new User
			{
				Name = dto.Name,
				Email = dto.Email,
				PasswordHash = passwordHash,
				Role = "Doctor",
				ImageUrl = imagePath
			};


			_context.users.Add(user);
			await _context.SaveChangesAsync();

			var doctor = new Doctor
			{
				Name = dto.Name,
				Specialization = dto.Specialization,
				ExperienceYears = dto.ExperienceYears,
				ConsultationPrice = dto.ConsultationPrice,
				StartTime = startTime,
				EndTime = endTime,
				DepartmentId = dto.DepartmentId,
				ImageURL = imagePath,
				Rating = 0,
				UserId = user.UserId
			};

			_context.Doctors.Add(doctor);
			await _context.SaveChangesAsync();

			return Ok("Doctor Added Successfully");
		}

		[HttpGet("hospitals")]
		public async Task<IActionResult> GetHospitals()
		{
			var hospitals = await _context.Hospitals
				.Select(h => new
				{
					h.HospitalId,
					h.Name
				})
				.ToListAsync();

			return Ok(hospitals);
		}

		[HttpGet("departments/by-hospital/{hospitalId}")]
		public async Task<IActionResult> GetDepartmentsByHospital(int hospitalId)
		{
			var departments = await _context.Departments
				.Where(d => d.HospitalId == hospitalId)
				.Select(d => new
				{
					d.DepartmentId,
					d.Name
				})
				.ToListAsync();

			return Ok(departments);
		}


		[HttpGet("search-doctors")]
		public async Task<IActionResult> GetDoctors(string? name, string? specialization)
		{
			var query = _context.Doctors
				.Include(d => d.Department)
				.ThenInclude(dep => dep.Hospital)
				.AsQueryable();

			// 🔍 فلترة بالاسم
			if (!string.IsNullOrEmpty(name))
			{
				query = query.Where(d => d.Name.Contains(name));
			}

			// 🔍 فلترة بالتخصص
			if (!string.IsNullOrEmpty(specialization))
			{
				query = query.Where(d => d.Specialization.Contains(specialization));
			}

			var doctors = await query
				.Select(d => new
				{
					d.DoctorId,
					d.Name,
					d.Specialization,
					d.ExperienceYears,
					d.Rating,
					d.ConsultationPrice,
					d.ImageURL,
					Department = d.Department.Name,
					Hospital = d.Department.Hospital.Name
				})
				.ToListAsync();

			return Ok(doctors);
		}

		[HttpGet("equipments")]
		public async Task<IActionResult> GetEquipments()
		{
			var query = _context.Equipments
				.Include(e => e.Owner)
				.Include(e => e.Rentals)
				.AsQueryable();

			var equipments = await query
				.Select(e => new
				{
					e.EquipmentId,
					e.Name,
					e.Description,
					e.PricePerDay,
					e.ImageUrl,

					// 👇 اسم المالك
					OwnerName = e.Owner != null ? e.Owner.Name : "No Owner",

					// 👇 الحالة نستنتجها
					Status =
						e.IsUnderMaintenance ? "Maintenance" :
						e.Availability ? "Availabe" :
						"booked",

					// 👇 إجمالي مرات التأجير
					TotalRentals = e.Rentals.Count(),

					// 👇 إجمالي الإيراد
					TotalRevenue = e.Rentals
						.Where(r => r.Status == "Booked")
						.Sum(r => (decimal?)r.TotalPrice) ?? 0
				})
				.ToListAsync();

			return Ok(equipments);
		}

		[HttpGet("search-equipments")]
		public async Task<IActionResult> SearchEquipments(string? name)
		{
			var query = _context.Equipments
				.Include(e => e.Owner)
				.Include(e => e.Rentals)
				.AsQueryable();

			// 🔍 بحث بالاسم فقط
			if (!string.IsNullOrEmpty(name))
			{
				query = query.Where(e => e.Name.Contains(name));
			}

			var equipments = await query
				.Select(e => new
				{
					e.EquipmentId,
					e.Name,
					e.Description,
					e.PricePerDay,
					e.ImageUrl,

					// 👇 اسم المالك
					OwnerName = e.Owner != null ? e.Owner.Name : "No Owner",

					// 👇 الحالة نستنتجها
					Status =
						e.IsUnderMaintenance ? "Maintenance" :
						e.Availability ? "Availabe" :
						"booked",

					// 👇 إجمالي مرات التأجير
					TotalRentals = e.Rentals.Count(),

					// 👇 إجمالي الإيراد
					TotalRevenue = e.Rentals
						.Where(r => r.Status == "Booked")
						.Sum(r => (decimal?)r.TotalPrice) ?? 0
				})
				.ToListAsync();

			return Ok(equipments);
		}


		[HttpPost("add-device")]
		public async Task<IActionResult> AddDevice([FromForm] AdminAddEquipmentDto model)
		{
			if (model == null)
				return BadRequest("Invalid Data");

			if (await _context.users.AnyAsync(u => u.Email == model.OwnerEmail))
				return BadRequest("Email already exists");

			// 🔐 اعملي Hash للباسورد
			var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.OwnerPassword);

			var owner = new User
			{
				Name = model.OwnerName,
				Email = model.OwnerEmail,
				PasswordHash = passwordHash,
				Phone = model.OwnerPhone,
				Role = "EquipmentOwner"
			};

			_context.users.Add(owner);
			await _context.SaveChangesAsync(); // عشان ياخد UserId

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
				Availability = true,
				IsUnderMaintenance = false,
				ImageUrl = imagePath,
				OwnerId = owner.UserId   // 👈 ربطناه باليوزر
			};

			_context.Equipments.Add(equipment);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Device & Owner Added Successfully" });
		}


		[HttpGet("operations")]
		public async Task<IActionResult> GetAllOperations()
		{
			// ===== Bookings (Doctors) =====
			var bookingsQuery = _context.Booking
				.Include(b => b.User)
				.Include(b => b.Doctor)
				.AsQueryable();


			var bookings = await bookingsQuery
				.Select(b => new AdminOperationDto
				{
					Id = b.BookingId,
					CustomerName = b.User.Name,
					Type = "Examination",
					ItemName = b.Doctor.Name,
					Date = b.BookingDate,
					Time = b.BookingDate.ToString("hh:mm tt"),
					Amount = b.Price,
					Status = b.Status
				})
				.ToListAsync();


			// ===== Rentals (Equipments) =====
			var rentalsQuery = _context.Rental
				.Include(r => r.User)
				.Include(r => r.Equipment)
				.AsQueryable();


			var rentals = await rentalsQuery
				.Select(r => new AdminOperationDto
				{
					Id = r.RentalId,
					CustomerName = r.User.Name,
					Type = "Rental",
					ItemName = r.Equipment.Name,
					Date = r.StartDate,
					Time = r.StartDate.ToString("hh:mm tt"),
					Amount = r.TotalPrice,
					Status = r.Status
				})
				.ToListAsync();


			// ===== دمج الاتنين =====
			var allOperations = bookings
				.Concat(rentals)
				.OrderByDescending(o => o.Date)
				.ToList();

			return Ok(allOperations);
		}

		[HttpGet("search-operations")]
		public async Task<IActionResult> GetAllOperations(string? status, string? search)
		{
			// ===== Bookings (Doctors) =====
			var bookingsQuery = _context.Booking
				.Include(b => b.User)
				.Include(b => b.Doctor)
				.AsQueryable();

			// فلترة بالحالة
			if (!string.IsNullOrEmpty(status))
				bookingsQuery = bookingsQuery.Where(b => b.Status == status);

			// بحث بالاسم
			if (!string.IsNullOrEmpty(search))
				bookingsQuery = bookingsQuery.Where(b =>
					b.User.Name.Contains(search));

			var bookings = await bookingsQuery
				.Select(b => new AdminOperationDto
				{
					Id = b.BookingId,
					CustomerName = b.User.Name,
					Type = "Examination",
					ItemName = b.Doctor.Name,
					Date = b.BookingDate,
					Time = b.BookingDate.ToString("hh:mm tt"),
					Amount = b.Price,
					Status = b.Status
				})
				.ToListAsync();


			// ===== Rentals (Equipments) =====
			var rentalsQuery = _context.Rental
				.Include(r => r.User)
				.Include(r => r.Equipment)
				.AsQueryable();

			if (!string.IsNullOrEmpty(status))
				rentalsQuery = rentalsQuery.Where(r => r.Status == status);

			if (!string.IsNullOrEmpty(search))
				rentalsQuery = rentalsQuery.Where(r =>
					r.User.Name.Contains(search));

			var rentals = await rentalsQuery
				.Select(r => new AdminOperationDto
				{
					Id = r.RentalId,
					CustomerName = r.User.Name,
					Type = "Rental",
					ItemName = r.Equipment.Name,
					Date = r.StartDate,
					Time = r.StartDate.ToString("hh:mm tt"),
					Amount = r.TotalPrice,
					Status = r.Status
				})
				.ToListAsync();


			// ===== دمج الاتنين =====
			var allOperations = bookings
				.Concat(rentals)
				.OrderByDescending(o => o.Date)
				.ToList();

			return Ok(allOperations);
		}

		[HttpGet("users")]
		public async Task<IActionResult> GetUsers()
		{
			var query = _context.users.AsQueryable();

			var users = await query
				.Select(u => new AdminUserDto
				{
					UserId = u.UserId,
					Name = u.Name,
					Email = u.Email,
					Role = u.Role,

					TotalBookings =
						_context.Booking.Count(b => b.UserId == u.UserId) +
						_context.Rental.Count(r => r.UserId == u.UserId),

					Status = u.IsBlocked ? "Blocked" : "Active"
				})
				.ToListAsync();

			return Ok(users);
		}

		[HttpGet("search-users")]
		public async Task<IActionResult> GetUsers(string? search)
		{
			var query = _context.users.AsQueryable();

			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(u =>
					u.Name.Contains(search) ||
					u.Email.Contains(search));
			}

			// 👇 نرتب الأول قبل أي Select

			var users = await query
				.Select(u => new AdminUserDto
				{
					UserId = u.UserId,
					Name = u.Name,
					Email = u.Email,
					Role = u.Role,

					TotalBookings =
						_context.Booking.Count(b => b.UserId == u.UserId) +
						_context.Rental.Count(r => r.UserId == u.UserId),

					Status = u.IsBlocked ? "Blocked" : "Active"
				})
				.ToListAsync();

			return Ok(users);
		}

		[HttpPut("users/toggle-block/{id}")]
		public async Task<IActionResult> ToggleBlockUser(int id)
		{
			var user = await _context.users.FindAsync(id);

			if (user == null)
				return NotFound();

			user.IsBlocked = !user.IsBlocked;

			await _context.SaveChangesAsync();

			return Ok(new
			{
				message = user.IsBlocked ? "User Blocked" : "User Unblocked"
			});
		}

		//[HttpPost("fix-doctors-users")]
		//public async Task<IActionResult> FixDoctorsUsers()
		//{
		//	var doctors = await _context.Doctors
		//		.Where(d => d.UserId == null)
		//		.ToListAsync();

		//	foreach (var doctor in doctors)
		//	{
		//		var email = $"doctor{doctor.DoctorId}@hospital.com";
		//		var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

		//		var user = new User
		//		{
		//			Name = doctor.Name,
		//			Email = email,
		//			PasswordHash = passwordHash,
		//			Role = "Doctor",
		//			ImageUrl = doctor.ImageURL
		//		};

		//		_context.users.Add(user);
		//		await _context.SaveChangesAsync();

		//		doctor.UserId = user.UserId;
		//	}

		//	await _context.SaveChangesAsync();

		//	return Ok("All doctors fixed successfully");
		//}
	}
}
