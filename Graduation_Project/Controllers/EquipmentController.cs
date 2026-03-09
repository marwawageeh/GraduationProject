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
	[Route("api/[controller]")]
	[ApiController]
	public class EquipmentController : ControllerBase
	{
		private readonly AppDBContext _context;

        public EquipmentController(AppDBContext DB)
        {
            _context = DB;
        }

        [HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] string? name)
		{
			var query = _context.Equipments.AsQueryable();

			if (!string.IsNullOrEmpty(name))
				query = query.Where(e => e.Name.Contains(name));

			//if (minPrice.HasValue)
			//	query = query.Where(e => e.PricePerDay >= minPrice);

			//if (maxPrice.HasValue)
			//	query = query.Where(e => e.PricePerDay <= maxPrice);

			//if (available.HasValue)
			//	query = query.Where(e => e.Availability == available);

			return Ok(await query.ToListAsync());
		}


		[HttpGet("filter")]
		public async Task<IActionResult> Filter(decimal? minPrice, decimal? maxPrice, bool? available)
		{
			var query = _context.Equipments.AsQueryable();

			//// Search by name
			//if (!string.IsNullOrEmpty(name))
			//{
			//	query = query.Where(e => e.Name.Contains(name));
			//}

			// Price range
			if (minPrice.HasValue)
				query = query.Where(e => e.PricePerDay >= minPrice);

			if (maxPrice.HasValue)
				query = query.Where(e => e.PricePerDay <= maxPrice);

			// Availability filter
			if (available.HasValue)
				query = query.Where(e => e.Availability == available);

			//// Rating filter
			//if (minRating.HasValue)
			//	query = query.Where(e => e.Rating >= minRating);

			var result = await query.ToListAsync();

			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetEquipmentById(int id)
		{
			var equipment = await _context.Equipments
				.FirstOrDefaultAsync(e => e.EquipmentId == id);

			if (equipment == null)
				return NotFound();

			return Ok(equipment);
		}


		[HttpGet("{id}/reviews")]
		public async Task<IActionResult> GetReviews(int id)
		{
			var reviews = await _context.Reviews
				.Where(r => r.EquipmentId == id)
				.ToListAsync();

			return Ok(reviews);
		}

		[HttpGet("{id}/rating-summary")]
		public async Task<IActionResult> GetRatingSummary(int id)
		{
			var reviews = await _context.Reviews
				.Where(r => r.EquipmentId == id)
				.ToListAsync();

			if (!reviews.Any())
				return Ok(new { Average = 0, Count = 0 });

			var avg = reviews.Average(r => r.Rating);

			var distribution = reviews
				.GroupBy(r => r.Rating)
				.Select(g => new { Rating = g.Key, Count = g.Count() })
				.ToList();

			return Ok(new
			{
				Average = avg,
				Count = reviews.Count,
				Distribution = distribution
			});
		}

		[HttpPost("{id}/add-review")]
		public async Task<IActionResult> AddReview(int id, Review review)
		{
			review.EquipmentId = id;
			_context.Reviews.Add(review);
			await _context.SaveChangesAsync();
			return Ok();
		}



		[HttpGet("{id}/availability")]
		public async Task<IActionResult> GetAvailability(int id)
		{
			var rentals = await _context.Rental
				.Where(r => r.EquipmentId == id && r.Status == "Booked")
				.ToListAsync();

			var bookedDates = new List<string>();

			foreach (var r in rentals)
			{
				for (var d = r.StartDate; d <= r.EndDate; d = d.AddDays(1))
				{
					bookedDates.Add(d.ToString("yyyy-MM-dd"));
				}
			}

			return Ok(new
			{
				BookedDates = bookedDates
			});
		}

		//[HttpGet("{id}/availability")]
		//public async Task<IActionResult> GetAvailability(int id)
		//{
		//	var rentals = await _context.Rental
		//		.Where(r => r.EquipmentId == id && r.EndDate >= DateTime.Now)
		//		.ToListAsync();

		//	var bookedDates = new List<string>();

		//	foreach (var r in rentals)
		//	{
		//		for (var d = r.StartDate.Date; d <= r.EndDate.Date; d = d.AddDays(1))
		//		{
		//			bookedDates.Add(d.ToString("yyyy-MM-dd"));
		//		}
		//	}

		//	return Ok(new { bookedDates });
		//}



		//[HttpPost("rent")]
		//public async Task<IActionResult> RentEquipment(Rental rental)
		//{
		//	bool conflict = await _context.Rental.AnyAsync(r =>
		//		r.EquipmentId == rental.EquipmentId &&
		//		r.EndDate >= rental.StartDate &&
		//		r.StartDate <= rental.EndDate
		//	);

		//	if (conflict)
		//		return BadRequest("Equipment already booked");

		//	rental.Status = "Booked";
		//	_context.Rental.Add(rental);
		//	await _context.SaveChangesAsync();

		//	return Ok(rental);
		//}

		[Authorize]
		[HttpPost("rent")]
		public async Task<IActionResult> RentEquipment(RentRequestDto request)
		{
			// 1️⃣ Validate dates
			if (request.EndDate <= request.StartDate)
				return BadRequest("Invalid date range");

			// 2️⃣ Check equipment exists
			var equipment = await _context.Equipments.FindAsync(request.EquipmentId);
			if (equipment == null)
				return NotFound("Equipment not found");

			// 3️⃣ Check conflict
			bool conflict = await _context.Rental.AnyAsync(r =>
				r.EquipmentId == request.EquipmentId &&
				r.EndDate >= request.StartDate &&
				r.StartDate <= request.EndDate
			);

			if (conflict)
				return BadRequest("Equipment already booked");

			// 4️⃣ Calculate days & price
			int days = (request.EndDate - request.StartDate).Days;
			decimal totalPrice = days * equipment.PricePerDay;

			var userId = int.Parse(User.FindFirst("uid").Value);


			// 5️⃣ Create rental record
			var rental = new Rental
			{
				EquipmentId = request.EquipmentId,
				StartDate = request.StartDate,
				EndDate = request.EndDate,
				TotalPrice = totalPrice,
				Status = "PendingPayment",
				//UserId = 2 // ⚠️ لاحقًا من JWT
				UserId = userId
			};

			_context.Rental.Add(rental);
			await _context.SaveChangesAsync();

			// 6️⃣ Return to frontend
			return Ok(new
			{
				rentalId = rental.RentalId,
				totalPrice = rental.TotalPrice
			});
		}

	}
}
