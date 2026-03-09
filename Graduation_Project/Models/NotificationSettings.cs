namespace Graduation_Project.Models
{
	public class NotificationSettings
	{
		public int Id { get; set; }

		public int UserId { get; set; }

		public bool AppointmentReminders { get; set; } = true;

		public bool EquipmentRentalAlerts { get; set; } = true;

		public User User { get; set; }
	}
}
