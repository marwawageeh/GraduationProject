using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
	[Authorize(Roles = "Doctor")]
	[Route("api/[controller]")]
	[ApiController]
	public class DoctorProfileController : ControllerBase
	{
		private readonly AppDBContext _context;

        public DoctorProfileController(AppDBContext context)
        {
			_context = context;
		}

		//[HttpGet]
		//public async Task<IActionResult> DoctorInfo()
		//{
		//	var doctorId = int.Parse(User.FindFirst("uid")?.Value);

		//	var doc=await _context.Doctors.Include(d=>d.Department).FirstOrDefaultAsync(d=>d.DoctorId== doctorId);

		//	var Doctor = new DoctorInfo
		//	{
		//		DoctorID = doc.DoctorId,
		//		DoctorName=doc.Name,
		//		Specialize=doc.Department.Name,
		//		ImageURL=doc.ImageURL

		//	};

		//	return Ok(Doctor);
		//}

		[HttpGet("doctor/dashboard")]
		public async Task<IActionResult> GetDoctorDashboard()
		{
			var doctorId = int.Parse(User.FindFirst("uid")?.Value);
			//var claim = User.FindFirst("uid");

			//if (claim == null)
			//	return Unauthorized("User claim missing");

			//var ownerId = int.Parse(claim.Value);

			var today = DateTime.Today;
			var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
			var startOfMonth = new DateTime(today.Year, 1, 1);

			// إجمالي المرضى (Distinct)
			var totalPatients = await _context.Booking
				.Where(b => b.DoctorId == doctorId)
				.Select(b => b.UserId)
				.Distinct()
				.CountAsync();

			var tomorrow = today.AddDays(1);

			var todayBookings = await _context.Booking
				.Where(b => b.DoctorId == doctorId &&
							b.BookingDate >= today &&
							b.BookingDate < tomorrow)
				.CountAsync();

			// في الانتظار
			var waitingCount = await _context.Booking
				.Where(b => b.DoctorId == doctorId &&
							b.Status == "Pending")
				.CountAsync();

			// معدل الرضا (من Doctor.Rating)
			var rating = await _context.Doctors
				.Where(d => d.DoctorId == doctorId)
				.Select(d => d.Rating)
				.FirstOrDefaultAsync();

			// أنواع الحجوزات
			var bookingTypes = await _context.Booking
				.Where(b => b.DoctorId == doctorId)
				.GroupBy(b => b.BookingTypes)
				.Select(g => new
				{
					Type = g.Key,
					Count = g.Count()
				})
				.ToListAsync();



			var weeklyRaw = await _context.Booking
	             .Where(b => b.DoctorId == doctorId &&
			            b.BookingDate >= startOfWeek)
	             .GroupBy(b => b.BookingDate.Date)
	             .Select(g => new
	             {
		             Date = g.Key,
		             Count = g.Count()
	             })
	             .ToListAsync();


			var weeklyBookings = weeklyRaw
				.GroupBy(x => x.Date.DayOfWeek)
				.Select(g => new
				{
					Day = g.Key.ToString(),
					Count = g.Sum(x => x.Count)
				})
				.ToList();

			// المرضى الشهري
			var monthlyPatients = await _context.Booking
				.Where(b => b.DoctorId == doctorId &&
							b.BookingDate.Year == today.Year)
				.GroupBy(b => b.BookingDate.Month)
				.Select(g => new
				{
					Month = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			return Ok(new
			{
				totalPatients,
				todayBookings,
				waitingCount,
				rating,
				bookingTypes,
				weeklyBookings,
				monthlyPatients
			});
		}

		[HttpGet("doctor/AllPatientbookings")]
		public async Task<IActionResult> GetAllpateintBookings()
		{
			var doctorId = int.Parse(User.FindFirst("uid")?.Value);

			var query = _context.Booking
				.Where(b => b.DoctorId == doctorId);


			var result = await query
				.OrderByDescending(b => b.BookingDate)
				.Select(b => new DoctorBookingDtoDashboard
				{
					BookingId = b.BookingId,
					PatientName = b.PatientName,
					PatientPhone = b.PatientPhone,
					PatientEmail = b.PatientEmail,
					Date = b.BookingDate,
					Time = b.BookingTime,
					Type = b.BookingTypes,
					Status = b.Status
				})
				.ToListAsync();

			return Ok(result);
		}

		[HttpGet("doctor/bookings")]
		public async Task<IActionResult> GetDoctorBookings(string? status, string? search)
		{
			var doctorId = int.Parse(User.FindFirst("uid")?.Value);

			var query = _context.Booking
				.Where(b => b.DoctorId == doctorId);

			// Filter by status
			if (!string.IsNullOrEmpty(status) && status != "All")
			{
				query = query.Where(b => b.Status == status);
			}

			// Search by name or phone
			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(b =>
					b.PatientName.Contains(search) ||
					b.PatientPhone.Contains(search));
			}

			var result = await query
				.OrderByDescending(b => b.BookingDate)
				.Select(b => new DoctorBookingDtoDashboard
				{
					BookingId = b.BookingId,
					PatientName = b.PatientName,
					PatientPhone = b.PatientPhone,
					PatientEmail = b.PatientEmail,
					Date = b.BookingDate,
					Time = b.BookingTime,
					Type = b.BookingTypes,
					Status = b.Status
				})
				.ToListAsync();

			return Ok(result);
		}

		[HttpGet("doctor/booking/{bookingId}")]
		public async Task<IActionResult> GetBookingDetails(int bookingId)
		{
			var booking = await _context.Booking
				.Where(b => b.BookingId == bookingId)
				.Select(b => new BookingDetailsDto
				{
					BookingId = b.BookingId,
					PatientName = b.PatientName,
					PatientPhone = b.PatientPhone,
					PatientEmail = b.PatientEmail,
					Date = b.BookingDate,
					Time = b.BookingTime,
					Notes = b.Notes,
					Type = b.BookingTypes,
					Status = b.Status,
					Price = b.Price
				})
				.FirstOrDefaultAsync();

			if (booking == null)
				return NotFound();

			return Ok(booking);
		}


	}
}
