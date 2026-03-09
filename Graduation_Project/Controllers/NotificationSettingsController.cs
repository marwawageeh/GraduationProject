using Graduation_Project.Context;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationSettingsController : ControllerBase
	{
		private readonly AppDBContext _context;

		public NotificationSettingsController(AppDBContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetMySettings()
		{
			var userId = int.Parse(User.FindFirst("uid").Value);

			var settings = await _context.NotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == userId);

			if (settings == null)
			{
				settings = new NotificationSettings
				{
					UserId = userId
				};

				_context.NotificationSettings.Add(settings);
				await _context.SaveChangesAsync();
			}

			return Ok(settings);
		}

		[HttpPut]
		public async Task<IActionResult> UpdateSettings(UpdateNotificationSettingsDto model)
		{
			var userId = int.Parse(User.FindFirst("uid").Value);

			var settings = await _context.NotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == userId);

			if (settings == null)
				return NotFound();

			settings.AppointmentReminders = model.AppointmentReminders;
			settings.EquipmentRentalAlerts = model.EquipmentRentalAlerts;

			await _context.SaveChangesAsync();

			return Ok("Updated");
		}

	}
}
