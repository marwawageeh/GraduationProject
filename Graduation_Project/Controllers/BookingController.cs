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
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class BookingController : ControllerBase
	{
		private readonly AppDBContext _context;

		public BookingController(AppDBContext context)
		{
			_context = context;
		}

		// 1️⃣ Create Booking
		[HttpPost]
		public async Task<IActionResult> CreateBooking(CreateBookingDto dto)
		{
			var userId = int.Parse(User.FindFirst("uid")!.Value);

			var doctor = await _context.Doctors
				.Include(d => d.Department)
				.ThenInclude(dep => dep.Hospital)
				.FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId);


			if (doctor == null)
				return NotFound("Doctor not found");

			// Check conflict
			bool conflict = await _context.Booking.AnyAsync(b =>
				b.DoctorId == dto.DoctorId &&
				b.BookingDate.Date == dto.Date.Date &&
				b.BookingTime == TimeSpan.Parse(dto.Time) &&
				b.Status != "Canceled"
			);

			if (conflict)
				return BadRequest("This time slot is already booked");

			var booking = new Booking
			{
				UserId = userId,
				DoctorId = dto.DoctorId,
				BookingDate = dto.Date.Date,
				BookingTime = TimeSpan.Parse(dto.Time),
			    Notes = dto.Notes,
				Price = doctor.ConsultationPrice,
				Status = "Pending",
			};

			_context.Booking.Add(booking);
			await _context.SaveChangesAsync();

			return Ok(new { bookingId = booking.BookingId });
		}

		// 2️⃣ Booking Summary (صفحة الدفع)
		[HttpGet("{id}/summary")]
		public async Task<IActionResult> GetSummary(int id)
		{
			var booking = await _context.Booking
				.Include(b => b.Doctor)
				.ThenInclude(d => d.Department)
				.ThenInclude(dep => dep.Hospital)
				.FirstOrDefaultAsync(b => b.BookingId == id);

			if (booking == null)
				return NotFound();

			var dto = new BookingSummaryDto
			{
				BookingId = booking.BookingId,
				DoctorName = booking.Doctor.Name,
				DepartmentName = booking.Doctor.Department.Name,
				HospitalName = booking.Doctor.Department.Hospital.Name,
				Date = booking.BookingDate,
				Time = booking.BookingTime,
				Price = booking.Price
			};

			return Ok(dto);
		}
	}
}
