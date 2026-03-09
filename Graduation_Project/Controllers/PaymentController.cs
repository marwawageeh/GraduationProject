using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Graduation_Project.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly IPaymentService _paymentService;
		private readonly AppDBContext _context;

		public PaymentController(IPaymentService paymentService, AppDBContext context)
		{
			_paymentService = paymentService;
			_context = context;
		}


		//[HttpGet("{id}/summary")]
		//public async Task<IActionResult> GetSummary(int id)
		//{
		//	var rental = await _context.Rental
		//		.Include(r => r.Equipment)
		//		.FirstOrDefaultAsync(r => r.RentalId == id);

		//	if (rental == null) return NotFound();

		//	int days = (rental.EndDate - rental.StartDate).Days;
		//	decimal insurance = 50;
		//	decimal tax = rental.TotalPrice * 0.1m;

		//	return Ok(new
		//	{
		//		equipmentName = rental.Equipment.Name,
		//		imageUrl = rental.Equipment.ImageUrl,
		//		startDate = rental.StartDate,
		//		endDate = rental.EndDate,
		//		rentalDays = days,
		//		pricePerDay = rental.Equipment.PricePerDay,
		//		insuranceFee = insurance,
		//		tax = tax,
		//		totalPrice = rental.TotalPrice + insurance + tax
		//	});
		//}
		[HttpGet("{id}/summary")]
		public async Task<IActionResult> GetSummary(int id)
		{
			var rental = await _context.Rental
				.Include(r => r.Equipment)
				.FirstOrDefaultAsync(r => r.RentalId == id);

			if (rental == null) return NotFound();

			int days = (rental.EndDate - rental.StartDate).Days;
			if (days == 0) days = 1;

			decimal rentalFee = rental.TotalPrice;
			decimal insurance = 50;
			//decimal tax = rental.TotalPrice * 0.1m;
			decimal tax = rentalFee * 0.1m;
			decimal total = rentalFee + insurance + tax;

			return Ok(new
			{
				equipmentName = rental.Equipment.Name,
				imageUrl = rental.Equipment.ImageUrl,
				startDate = rental.StartDate,
				endDate = rental.EndDate,

				rentalDays = days,
				pricePerDay = rental.Equipment.PricePerDay,
				rentalFee = rentalFee,
				insuranceFee = insurance,
				tax = tax,
				totalPrice = total
			});
		}


		[HttpPost("start-checkout")]
		public async Task<IActionResult> StartCheckout(StartCheckoutDTO dto)
		{
			var rental = await _context.Rental.FindAsync(dto.RentalId);

			if (rental == null)
				return NotFound("Rental not found");

			//var userId = int.Parse(User.FindFirst("id").Value);
			var userIdClaim = User.FindFirst("uid");

			if (userIdClaim == null)
				return Unauthorized("User ID missing in token");

			var userId = int.Parse(userIdClaim.Value);

			if (dto == null)
				return BadRequest("Invalid data");

			if (rental.UserId != userId)
				return Unauthorized();

			if (rental.Status == "Booked")
				return BadRequest("Already paid");

			if (!string.IsNullOrEmpty(rental.PaymentIntentId))
				return BadRequest("Payment already initiated");

			var existing = await _context.DeliveryAddresses
				.FirstOrDefaultAsync(d => d.RentalId == dto.RentalId);

			if (existing == null)
			{
				var address = new DeliveryAddress
				{
					RentalId = dto.RentalId,
					FullName = dto.FullName,
					Phone = dto.Phone,
					StreetAddress = dto.StreetAddress,
					Apartment = dto.Apartment,
					City = dto.City
				};

				_context.DeliveryAddresses.Add(address);
			}

			var result = await _paymentService
				.CreatePaymentIntentAsync(rental.TotalPrice, rental.RentalId);

			rental.PaymentIntentId = result.PaymentIntentId;

			await _context.SaveChangesAsync();

			return Ok(result);
		}



		[HttpPost("verify/{paymentIntentId}")]
		public async Task<IActionResult> Verify(string paymentIntentId)
		{
			var success = await _paymentService.VerifyPaymentAsync(paymentIntentId);

			if (!success)
				return BadRequest("Payment Failed");

			var rental = await _context.Rental
				.Include(r => r.Equipment)
				.FirstOrDefaultAsync(r => r.PaymentIntentId == paymentIntentId);

			if (rental == null)
				return NotFound("Rental not found");

			if (rental.Status == "Booked")
				return BadRequest("Rental already confirmed");

			rental.Status = "Booked";


			// 🟢 1️⃣ إشعار المريض
			var patientSettings = await _context.NotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == rental.UserId);

			if (patientSettings?.EquipmentRentalAlerts == true)
			{
				_context.Notification.Add(new Notification
				{
					UserId = rental.UserId,
					Title = "Equipment Rental Confirmed",
					Message = $"Your {rental.Equipment.Name} rental is confirmed and will be delivered soon.",
					CreatedAt = DateTime.UtcNow,
					IsRead = false
				});
			}


			// 🟢 2️⃣ إشعار صاحب الجهاز
			if (rental.Equipment.OwnerId.HasValue)
			{
				var ownerSettings = await _context.NotificationSettings
					.FirstOrDefaultAsync(s => s.UserId == rental.Equipment.OwnerId.Value);

				if (ownerSettings?.EquipmentRentalAlerts == true)
				{
					_context.Notification.Add(new Notification
					{
						UserId = rental.Equipment.OwnerId.Value,
						Title = "New Equipment Rental",
						Message = $"Your equipment {rental.Equipment.Name} has been rented from {rental.StartDate:dd MMM yyyy} to {rental.EndDate:dd MMM yyyy}.",
						CreatedAt = DateTime.UtcNow,
						IsRead = false
					});
				}
			}

			await _context.SaveChangesAsync();

			return Ok("Payment Success");
		}


	}
}
