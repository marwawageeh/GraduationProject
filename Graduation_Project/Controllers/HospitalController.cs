using Graduation_Project.Context;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Graduation_Project.DTO;
using Graduation_Project.Service;
using System.Linq;
using Azure.Core;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Stripe;
using Microsoft.AspNetCore.Authorization;
using System.Numerics;

namespace Graduation_Project.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	public class HospitalController : ControllerBase
	{
		private readonly AppDBContext _context;
		private readonly AIQueryService _ai;
		private readonly SearchQueryParserService _parser;
		private readonly HospitalSearchService _search;

		public HospitalController(AppDBContext context, AIQueryService ai, SearchQueryParserService parser, HospitalSearchService search)
		{
			_context = context;
			_ai = ai;
			_parser = parser;
			_search = search;
		}


		//		[HttpPost("search")]
		//		public async Task<IActionResult> Search([FromBody] SearchRequest request)
		//		{
		//			if (string.IsNullOrWhiteSpace(request.Text))
		//				return BadRequest("Search text is required.");

		//			// 1️⃣ Get full hospital data
		//			var hospitals = await _context.Hospitals
		//				.Include(h => h.Departments)
		//				.Include(h => h.Equipments)
		//				.ToListAsync();

		//			if (!hospitals.Any())
		//				return Ok(new { success = true, results = new List<HospitalDto>() });

		//			// 2️⃣ Prepare hospital data for AI
		//			var hospitalDataJson = JsonSerializer.Serialize(
		//				hospitals.Select(h => new
		//				{
		//					h.HospitalId,
		//					h.Name,
		//					h.Address,
		//					h.Rating,
		//					h.ReservationPrice,
		//					Departments = h.Departments.Select(d => d.Name),
		//					Equipments = h.Equipments.Select(e => e.Name)
		//				}),
		//				new JsonSerializerOptions
		//				{
		//					WriteIndented = false
		//				}
		//			);

		//			// 3️⃣ AI prompt (STRICT)
		//			var prompt = $@"
		//أنت مساعد ذكي.
		//لديك قائمة المستشفيات التالية بصيغة JSON:

		//{hospitalDataJson}

		//مطلوب: اختر المستشفيات المناسبة لعبارة المستخدم:
		//""{request.Text}""

		//❗ تعليمات صارمة:
		//- رجّع JSON فقط
		//- لا تضف أي شرح أو نص
		//- لا تستخدم ```json
		//- الإخراج يجب أن يبدأ بـ [ وينتهي بـ ]

		//شكل الإخراج:
		//[
		//  {{
		//    ""Id"": 0,
		//    ""Name"": """",
		//    ""Address"": """",
		//    ""Rating"": 0,
		//    ""ReservationPrice"": 0,
		//    ""Departments"": [],
		//    ""Equipments"": []
		//  }}
		//]
		//";

		//			// 4️⃣ Call AI
		//			var aiResult = await _ai.GetAIResponse(prompt);

		//			// 5️⃣ Extract clean JSON
		//			var cleanJson = ExtractJson(aiResult);

		//			//if (string.IsNullOrWhiteSpace(cleanJson))
		//			//	return BadRequest("AI did not return valid JSON.");

		//			if (string.IsNullOrWhiteSpace(cleanJson))
		//			{
		//				// 🔁 Fallback DB Search
		//				var fallbackResults = hospitals
		//					.Where(h =>
		//						h.Name.Contains(request.Text) ||
		//						h.Address.Contains(request.Text))
		//					.Select(h => new HospitalDto
		//					{
		//						Id = h.HospitalId,
		//						Name = h.Name,
		//						Address = h.Address,
		//						Rating = h.Rating,
		//						ReservationPrice = h.ReservationPrice,
		//						Departments = h.Departments.Select(d => d.Name).ToList(),
		//						Equipments = h.Equipments.Select(e => e.Name).ToList()
		//					})
		//					.ToList();

		//				return Ok(new
		//				{
		//					success = true,
		//					results = fallbackResults
		//				});
		//			}


		//			// 6️⃣ Deserialize
		//			List<HospitalDto>? result;
		//			try
		//			{
		//				result = JsonSerializer.Deserialize<List<HospitalDto>>(
		//					cleanJson,
		//					new JsonSerializerOptions
		//					{
		//						PropertyNameCaseInsensitive = true
		//					}
		//				);
		//			}
		//			catch (Exception ex)
		//			{
		//				return BadRequest("Failed to parse AI JSON: " + ex.Message);
		//			}

		//			return Ok(new
		//			{
		//				success = true,
		//				results = result ?? new List<HospitalDto>()
		//			});
		//		}



		private string ExtractJson(string aiText)
		{
			if (string.IsNullOrWhiteSpace(aiText))
				return string.Empty;

			var start = aiText.IndexOf('[');
			var end = aiText.LastIndexOf(']');

			if (start == -1 || end == -1 || end < start)
				return string.Empty;

			return aiText.Substring(start, end - start + 1);
		}

		//[HttpGet("search")]
		//public async Task<IActionResult> Search(string query)
		//{
		//	var result = await _search.Search(query);
		//	return Ok(result);
		//}
		private double CalculateDistance(
			double lat1, double lon1,
			double lat2, double lon2)
		{
			var R = 6371;
			var dLat = (lat2 - lat1) * Math.PI / 180;
			var dLon = (lon2 - lon1) * Math.PI / 180;

			var a =
				Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(lat1 * Math.PI / 180) *
				Math.Cos(lat2 * Math.PI / 180) *
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R * c;
		}

		// 👇 حطيها هنا
		private async Task<string?> ExpandShortUrl(string shortUrl)
		{
			try
			{
				var handler = new HttpClientHandler
				{
					AllowAutoRedirect = false
				};

				using var client = new HttpClient(handler);
				var response = await client.GetAsync(shortUrl);

				if (response.StatusCode == System.Net.HttpStatusCode.Found ||
					response.StatusCode == System.Net.HttpStatusCode.Moved)
				{
					return response.Headers.Location?.ToString();
				}

				return null;
			}
			catch
			{
				return null;
			}
		}
		private (double?, double?) ExtractCoordinates(string url)
		{
			var regex = new System.Text.RegularExpressions.Regex(@"@(-?\d+\.\d+),(-?\d+\.\d+)");
			var match = regex.Match(url);

			if (match.Success)
			{
				return (
					double.Parse(match.Groups[1].Value),
					double.Parse(match.Groups[2].Value)
				);
			}

			var regex2 = new System.Text.RegularExpressions.Regex(@"q=(-?\d+\.\d+),(-?\d+\.\d+)");
			var match2 = regex2.Match(url);

			if (match2.Success)
			{
				return (
					double.Parse(match2.Groups[1].Value),
					double.Parse(match2.Groups[2].Value)
				);
			}

			return (null, null);
		}



		[HttpPost("search")]
		public async Task<IActionResult> Search([FromBody] SearchRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest("Search text is required.");

			var parsed = await _parser.Parse(request.Text);

			// 🔥 لو المستخدم دخل Google Maps Short Link
			if (request.Text.Contains("maps.app.goo.gl"))
			{
				var expanded = await ExpandShortUrl(request.Text);

				if (!string.IsNullOrEmpty(expanded))
				{
					var coords = ExtractCoordinates(expanded);

					if (coords.Item1.HasValue && coords.Item2.HasValue)
					{
						parsed.Latitude = coords.Item1;
						parsed.Longitude = coords.Item2;
					}
				}
			}


			var query = _context.Hospitals
			 .Include(h => h.Departments)
			 .Include(h => h.Equipments)
			 .Select(h => new
			 {
				 Hospital = h,
				 AverageRating = _context.HospitalReviews
					   .Where(r => r.HospitalId == h.HospitalId)
					   .Average(r => (double?)r.Rating) ?? 0
			 });


			//if (!string.IsNullOrEmpty(parsed.Name))
			//   query = query.Where(h =>
			//		h.Hospital.Name.ToLower().Contains(parsed.Name));

			//if (!string.IsNullOrEmpty(parsed.Name))
			//{
			//	var name = parsed.Name.ToLower().Trim();

			//	query = query.Where(h =>
			//		h.Hospital.Name.ToLower().Contains(name));
			//}

			//if (!string.IsNullOrEmpty(parsed.Name))
			//{
			//	var name = parsed.Name.Trim();

			//	query = query.Where(h =>
			//		EF.Functions.Like(h.Hospital.Name, $"%{name}%"));
			//}

			if (!string.IsNullOrEmpty(parsed.Name))
			{
				var name = parsed.Name.Trim().Replace("  ", " ");

				query = query.Where(h =>
					EF.Functions.Like(
						h.Hospital.Name.Replace("  ", " "),
						$"%{name}%"));
			}




			if (!string.IsNullOrEmpty(parsed.Location))
			{
				var loc = parsed.Location.ToLower();
				query = query.Where(h =>
					h.Hospital.Address.ToLower().Contains(loc));
			}

			if (!string.IsNullOrEmpty(parsed.Department))
				query = query.Where(h =>
					h.Hospital.Departments.Any(d => d.Name.Contains(parsed.Department)));

			if (!string.IsNullOrEmpty(parsed.Device))
				query = query.Where(h =>
					h.Hospital.Equipments.Any(e => e.Name.Contains(parsed.Device)));

			if (parsed.MaxPrice.HasValue)
				query = query.Where(h =>
					h.Hospital.ReservationPrice <= parsed.MaxPrice.Value);

			if (parsed.MinRating.HasValue)
			{
				query = query.Where(h =>
					_context.HospitalReviews
						.Where(r => r.HospitalId == h.Hospital.HospitalId)
						.Average(r => (double?)r.Rating) >= parsed.MinRating.Value);
			}

			//var hospitals = await query
			//           .ToListAsync();


			//if (parsed.Latitude.HasValue && parsed.Longitude.HasValue)
			//{
			//	foreach (var h in query)
			//	{
			//		h.Hospital.Distance = CalculateDistance(
			//			parsed.Latitude.Value,
			//			parsed.Longitude.Value,
			//			h.Hospital.Latitude,
			//			h.Hospital.Longitude);
			//	}
			//}

			var hospitals = await query.ToListAsync();

			if (parsed.Latitude.HasValue && parsed.Longitude.HasValue)
			{
				foreach (var h in hospitals)
				{
					h.Hospital.Distance = CalculateDistance(
						parsed.Latitude.Value,
						parsed.Longitude.Value,
						h.Hospital.Latitude,
						h.Hospital.Longitude);
				}

				hospitals = hospitals
					.OrderBy(h => h.Hospital.Distance)
					.ToList();
			}


			//	hospitals = hospitals
			//		.OrderBy(h => h.Hospital.Distance)
			//		.ToList();
			//}


			//var reviews = await _context.HospitalReviews
			// .ToListAsync();

			//var avg = reviews.Average(r => r.Rating);

			//if (!reviews.Any())
			//	return Ok(new { Average = 0, Count = 0 });

			//			var results = await query
			//				.Select(h => new HospitalDto
			//				{
			//					Id = h.Hospital.HospitalId,
			//					Name = h.Hospital.Name,
			//					Address = h.Hospital.Address,
			//					Average = _context.HospitalReviews
			//									  .Where(r => r.HospitalId == h.Hospital.HospitalId)
			//									  .Average(r => (double?)r.Rating) ?? 0
			//,
			//					ImageUrl = h.Hospital.ImageUrl,
			//					Count = _context.HospitalReviews
			//									.Count(r => r.HospitalId == h.Hospital.HospitalId),

			//					Description = h.Hospital.Description,
			//					ReservationPrice = h.Hospital.ReservationPrice,
			//					Departments = h.Hospital.Departments.Select(d => d.Name).ToList(),
			//					Equipments = h.Hospital.Equipments.Select(e => e.Name).ToList()
			//				})
			//				.ToListAsync();

			var results = hospitals
	.Select(h => new HospitalDto
	{
		Id = h.Hospital.HospitalId,
		Name = h.Hospital.Name,
		Address = h.Hospital.Address,
		Average = h.AverageRating,
		ImageUrl = h.Hospital.ImageUrl,
		Count = _context.HospitalReviews
			.Count(r => r.HospitalId == h.Hospital.HospitalId),
		Description = h.Hospital.Description,
		ReservationPrice = h.Hospital.ReservationPrice,
		Departments = h.Hospital.Departments.Select(d => d.Name).ToList(),
		Equipments = h.Hospital.Equipments.Select(e => e.Name).ToList()
	})
	.ToList();


			return Ok(new { success = true, results });
			}
		

		[HttpGet("{id}")]
		public async Task<IActionResult> GetHospitalbyId(int id)
		{
			var hospital = await _context.Hospitals
				.Include(h => h.Departments)
				.FirstOrDefaultAsync(h => h.HospitalId == id);

			if (hospital == null)
				return NotFound();


			var reviews = await _context.HospitalReviews
						 .Where(r => r.HospitalId == id)
						 .ToListAsync();


			var avg = reviews.Any()
	                       ? reviews.Average(r => r.Rating)
	                    : 0;


			//var avg = reviews.Average(r => r.Rating);


			var results = new HospitalDto
			{
				Id = id,
				Name = hospital.Name,
				Address = hospital.Address,
				ReservationPrice = hospital.ReservationPrice,
				ImageUrl = hospital.ImageUrl,
				Departments = hospital?.Departments?.Select(d => d.Name).ToList(),
				Average = avg,
				Count = reviews.Count,
				Description = hospital.Description,
				Direction = $"https://www.google.com/maps?q={hospital.Latitude},{hospital.Longitude}"

			};


			return Ok(results);

		}

		[HttpGet("{id}/reviews")]
		public async Task<IActionResult> GetHospitalReviews(int id)
		{
			var reviews = await _context.HospitalReviews
				.Where(r => r.HospitalId == id)
				.ToListAsync();


			return Ok(reviews);
		}


		[HttpGet("{id}/booking-details")]
		public async Task<IActionResult> GetBookingDetails(int id)
		{
			var hospital = await _context.Hospitals
				.Include(h => h.Departments)
					.ThenInclude(d => d.Doctors)
				.FirstOrDefaultAsync(h => h.HospitalId == id);

			if (hospital == null)
				return NotFound();

			var result = new HospitalBookingDetailsDto
			{
				HospitalId = hospital.HospitalId,
				HospitalName = hospital.Name,
				Departments = hospital.Departments.Select(d => new DepartmentBookingDto
				{
					DepartmentId = d.DepartmentId,
					Name = d.Name,
					Doctors = d.Doctors.Select(doc => new DoctorBookingDto
					{
						DoctorId = doc.DoctorId,
						Name = doc.Name,
						Specialization = doc.Specialization,
						ExperienceYears = doc.ExperienceYears,
						ImageURL = doc.ImageURL,
					}).ToList()
				}).ToList()
			};

			return Ok(result);
		}



		[HttpGet("doctor/{doctorId}/available-times")]
		public async Task<IActionResult> GetDoctorAvailableTimes(
	         int doctorId,
	         DateTime date)
		{
			var doctor = await _context.Doctors
				.FirstOrDefaultAsync(d => d.DoctorId == doctorId);

			if (doctor == null)
				return NotFound();

			var slots = new List<string>();
			var current = doctor.StartTime;

			while (current.Add(TimeSpan.FromMinutes(30)) <= doctor.EndTime)
			{
				slots.Add(current.ToString(@"hh\:mm"));
				current = current.Add(TimeSpan.FromMinutes(30));
			}

			var booked = await _context.Booking
				.Where(b =>
					b.DoctorId == doctorId &&
					b.BookingDate.Date == date.Date)
				.Select(b => b.BookingDate.ToString("HH:mm"))
				.ToListAsync();

			var available = slots.Except(booked).ToList();

			return Ok(available);
		}



	}

}

