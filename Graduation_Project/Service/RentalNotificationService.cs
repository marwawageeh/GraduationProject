using Graduation_Project.Context;
using Graduation_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Service
{
	public class RentalNotificationService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public RentalNotificationService(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = _scopeFactory.CreateScope();
				var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();

				var rentals = await context.Rental
					.Where(r => r.Status == "Booked" && !r.EndingSoonNotified)
					.ToListAsync();

				foreach (var rental in rentals)
				{
					var remaining = rental.EndDate - DateTime.UtcNow;

					if (remaining.TotalHours <= 48 && remaining.TotalHours > 0)
					{
						context.Notification.Add(new Notification
						{
							UserId = rental.UserId,
							Title = "Rental Ending Soon",
							Message = "Your rental will end soon. Extend or request pickup.",
							CreatedAt = DateTime.UtcNow,
							IsRead = false
						});

						rental.EndingSoonNotified = true;
					}
				}

				await context.SaveChangesAsync();

				//await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
				await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

			}
		}
	}
}
