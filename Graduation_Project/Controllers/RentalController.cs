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
	[Route("api/[controller]")]
	[ApiController]
	public class RentalController : ControllerBase
	{

		private readonly AppDBContext _context;
		private readonly IPaymentService _paymentService;

		public RentalController(AppDBContext DB, IPaymentService paymentService)
        {
			_context = DB;
			_paymentService = paymentService;
		}


		[HttpGet("{id}/countdown")]
		public async Task<IActionResult> GetCountdown(int id)
		{
			var rental = await _context.Rental.FindAsync(id);

			if (rental == null)
				return NotFound();

			var remaining = (rental.EndDate - DateTime.UtcNow);

			var dto = new RentalCountdownDTO
			{
				RentalId = rental.RentalId,
				RemainingHours = remaining.TotalHours,
				RemainingDays = remaining.TotalDays,
				IsExpired = remaining.TotalSeconds <= 0
			};

			return Ok(dto);
		}


		[HttpGet("{id}/status")]
		public async Task<IActionResult> GetRentalStatus(int id)
		{
			var rental = await _context.Rental.FindAsync(id);

			if (rental == null)
				return NotFound("Rental not found");

			if (rental.Status != "Booked")
				return BadRequest("Rental not active");

			var remaining = rental.EndDate - DateTime.UtcNow;

			double hours = remaining.TotalHours;

			bool isExpired = hours <= 0;

			bool isEndingSoon = hours <= 48 && hours > 0;

			var result = new RentalStatusDTO
			{
				RentalId = rental.RentalId,
				RemainingHours = hours,
				IsExpired = isExpired,
				IsEndingSoon = isEndingSoon
			};

			return Ok(result);
		}


		[Authorize]
		[HttpPost("{id}/extend")]
		public async Task<IActionResult> ExtendRental(int id, int extraDays)
		{
			var claim = User.FindFirst("uid");

			if (claim == null)
				return Unauthorized("User claim missing");

			var userId = int.Parse(claim.Value);

			var rental = await _context.Rental
				.Include(r => r.Equipment)
				.FirstOrDefaultAsync(r => r.RentalId == id);

			if (rental == null)
				return NotFound("Rental not found");

			if (rental.UserId != userId)
				return Unauthorized();

			if (rental.Status != "Booked")
				return BadRequest("Rental not active");

			if (rental.Equipment == null)
				return BadRequest("Equipment missing");

			rental.EndDate = rental.EndDate.AddDays(extraDays);

			var extraPrice = rental.Equipment.PricePerDay * extraDays;

			var payment = await _paymentService
				.CreatePaymentIntentAsync(extraPrice, rental.RentalId);

			await _context.SaveChangesAsync();

			return Ok(new
			{
				newEndDate = rental.EndDate,
				clientSecret = payment.ClientSecret
			});
		}


		[HttpPost("request-pickup/{id}")]
		public async Task<IActionResult> RequestPickup(int id)
		{
			var rental = await _context.Rental.FindAsync(id);

			if (rental == null)
				return NotFound("Rental not found");

			if (rental.Status != "Booked")
				return BadRequest("Pickup can only be requested for booked rentals");

			rental.Status = "PickupRequested";

			_context.Notification.Add(new Notification
			{
				UserId = rental.UserId,
				Title = "Pickup Requested",
				Message = "Your pickup request has been sent.",
				CreatedAt = DateTime.UtcNow,
				IsRead = false
			});

			await _context.SaveChangesAsync();

			return Ok("Pickup requested successfully");
		}



	}
}
