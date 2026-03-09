using Graduation_Project.Context;
using Graduation_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Service
{
	public class HospitalSearchService
	{
		private readonly AppDBContext _context;
		private readonly SearchQueryParserService _parser;

		public HospitalSearchService(
			AppDBContext context,
			SearchQueryParserService parser)
		{
			_context = context;
			_parser = parser;
		}

		public async Task<List<Hospital>> Search(string input)
		{
			// 1️⃣ Normalize
			var normalized = TextNormalizer.Normalize(input);

			// 2️⃣ Detect Department locally (سريع)
			var detectedDepartment =
				DepartmentKeywordMapper.DetectDepartment(normalized);

			// 3️⃣ AI parsing (للحاجات المعقدة)
			var aiResult = await _parser.Parse(input);

			var query = _context.Hospitals
				.Include(h => h.Departments)
				.Include(h => h.Reviews)
				.AsQueryable();

			// ✅ Department من Local Layer
			if (!string.IsNullOrEmpty(detectedDepartment))
			{
				query = query.Where(h =>
					h.Departments.Any(d =>
						d.Name == detectedDepartment));
			}
			// ✅ أو من AI
			else if (!string.IsNullOrEmpty(aiResult.Department))
			{
				query = query.Where(h =>
					h.Departments.Any(d =>
						d.Name == aiResult.Department));
			}

			// ✅ Rating
			if (aiResult.MinRating.HasValue)
			{
				query = query.Where(h =>
					h.Reviews.Any() &&
					h.Reviews.Average(r => r.Rating)
						>= aiResult.MinRating.Value);
			}

			// ✅ Location
			if (!string.IsNullOrEmpty(aiResult.Location))
			{
				query = query.Where(h =>
					h.Address.Contains(aiResult.Location));
			}

			// ✅ Name search
			query = query.Where(h =>
				h.Name.ToLower().Contains(normalized));

			return await query.ToListAsync();
		}
	}
}
