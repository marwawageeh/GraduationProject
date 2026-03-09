using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ChatController : ControllerBase
	{
		private readonly ChatService _chatService;
		private readonly AppDBContext _context;

		public ChatController(ChatService chatService, AppDBContext context)
		{
			_chatService = chatService;
			_context = context;
		}



		[HttpPost("analyze")]
		public async Task<IActionResult> Analyze([FromBody] ChatRequest request)
		{


			// 1️⃣ جيب الأقسام من الداتا بيز
			var allowedDepartments =
				await _context.Departments
				.Select(d => d.Name)
				.ToListAsync();

			// 2️⃣ تحليل AI
			var aiResult =
				await _chatService.AnalyzeSymptomsGroq(
					request.Message,
					allowedDepartments);

			var city = NormalizeCity(aiResult.City);

			// 3️⃣ جيب المستشفيات
			var hospitals = await _context.Hospitals
				.Include(h => h.Departments)
				.Include(h => h.Reviews)
				.Where(h =>
					(city == null || h.Address.ToLower().Contains(city))
					&&
					h.Departments.Any(d => d.Name == aiResult.Department))
				.Select(h => new NearbyHospitalDto
				{
					HospitalId = h.HospitalId,
					Name = h.Name,
					//DistanceKm = 2.5,
					Count = _context.HospitalReviews
			               .Count(r => r.HospitalId == h.HospitalId),
					Rating = h.Reviews.Any()
						? h.Reviews.Average(r => r.Rating)
						: 0
				})
				.ToListAsync();

			Console.WriteLine("AI City = " + aiResult.City);
			Console.WriteLine("Normalized City = " + city);

			var result = new MedicalAnalysisResultDto
			{
				Department = aiResult.Department,
				SuggestedActions = aiResult.SuggestedActions,
				HowToUse = aiResult.HowToUse,
				Hospitals = hospitals
			};

			return Ok(result);
		}

		private string NormalizeCity(string city)
		{
			if (string.IsNullOrEmpty(city))
				return null;

			var map = new Dictionary<string, string>()
	{
		{ "طنطا", "Tanta" },
		{ "القاهره", "Cairo" },
		{ "القاهرة", "Cairo" },
		{ "المنصوره", "Mansoura" },
		{ "المنصورة", "Mansoura" },
		{ "اسكندريه", "Alexandria" }
	};

			city = city.ToLower().Trim();

			return map.ContainsKey(city) ? map[city] : city;
		}
	}
}
