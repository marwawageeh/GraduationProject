using Graduation_Project.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

	}
}
