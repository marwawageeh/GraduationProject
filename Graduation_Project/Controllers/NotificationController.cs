using Graduation_Project.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationController : ControllerBase
	{
		private readonly AppDBContext _context;

		public NotificationController(AppDBContext context)
		{
			_context = context;
		}

		[HttpGet("my")]
		public async Task<IActionResult> GetMyNotifications()
		{
			var userId = int.Parse(User.FindFirst("uid").Value);


			var notifications = await _context.Notification
				.Where(n => n.UserId == userId)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();

			return Ok(notifications);
		}
	}
}
