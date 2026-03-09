using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class BookingPaymentController : ControllerBase
	{
		private readonly AppDBContext _context;
		private readonly IPaymentService _paymentService;

		public BookingPaymentController(AppDBContext context, IPaymentService paymentService)
		{
			_context = context;
			_paymentService = paymentService;
		}

		// Create Payment Intent
		[HttpPost("create-intent/{bookingId}")]
		public async Task<IActionResult> CreateIntent(int bookingId)
		{
			var booking = await _context.Booking.FindAsync(bookingId);
			if (booking == null)
				return NotFound();

			if (!string.IsNullOrEmpty(booking.PaymentIntentId))
				return BadRequest("Payment already initiated");


			var result = await _paymentService.CreatePaymentIntentAsync(
				booking.Price,
				"BookingId",
				bookingId
			);

			booking.PaymentIntentId = result.PaymentIntentId;
			await _context.SaveChangesAsync();

			return Ok(result);
		}


		[HttpPost("verify")]
		public async Task<IActionResult> Verify(VerifyBookingPaymentDto dto)
		{
			var success = await _paymentService.VerifyPaymentAsync(dto.PaymentIntentId);
			if (!success)
				return BadRequest("Payment Failed");


			var booking = await _context.Booking
				.FirstOrDefaultAsync(b => b.PaymentIntentId == dto.PaymentIntentId);

			if (booking == null)
				return NotFound();

			if (booking.Status == "Confirmed")
				return BadRequest("Booking already confirmed");

			booking.Status = "Confirmed";

			booking.PatientName = dto.PatientName;
			booking.PatientEmail = dto.PatientEmail;
			booking.PatientPhone = dto.PatientPhone;
			booking.BookingTypes = dto.BookingType;


			// نجيب اسم الدكتور والتاريخ
			await _context.Entry(booking)
				.Reference(b => b.Doctor)
				.LoadAsync();


			booking.Status = "Confirmed";


			// 🟢 1️⃣ إشعار المريض (AppointmentReminders)
			var patientSettings = await _context.NotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == booking.UserId);

			if (patientSettings?.AppointmentReminders == true)
			{
				_context.Notification.Add(new Notification
				{
					UserId = booking.UserId,
					Title = "Doctor Consultation Confirmed",
					Message = $"Your appointment with Dr. {booking.Doctor.Name} is scheduled for {booking.BookingTime} on {booking.BookingDate:dd MMM yyyy}.",
					CreatedAt = DateTime.UtcNow,
					IsRead = false
				});
			}


			// 🟢 2️⃣ إشعار الدكتور
			if (booking.Doctor.UserId.HasValue)
			{
				var doctorSettings = await _context.NotificationSettings
					.FirstOrDefaultAsync(s => s.UserId == booking.Doctor.UserId.Value);

				if (doctorSettings?.AppointmentReminders == true)
				{
					_context.Notification.Add(new Notification
					{
						UserId = booking.Doctor.UserId.Value,
						Title = "New Booking Received",
						Message = $"You have a new booking from {booking.PatientName} at {booking.BookingTime} on {booking.BookingDate:dd MMM yyyy}.",
						CreatedAt = DateTime.UtcNow,
						IsRead = false
					});
				}
			}


			_context.Payment.Add(new Payment
			{
				Amount = booking.Price,
				BookingId = booking.BookingId,
				Date = DateTime.Now,
				Method = "Card",
				Status = "Success"
			});

			await _context.SaveChangesAsync();

			return Ok("Payment Success");
		}

	}
}
