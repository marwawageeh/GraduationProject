namespace Graduation_Project.DTO
{
	public class OwnerDashboardDto
	{
		public int TotalDevices { get; set; }
		public int TodayRentals { get; set; }
		public int PendingRentals { get; set; }
		public double Rating { get; set; }

		public List<WeeklyRentalDto> WeeklyRentals { get; set; }
		public List<RentalTypeDto> RentalTypes { get; set; }
	}
	public class WeeklyRentalDto
	{
		public string Day { get; set; }
		public int Count { get; set; }
	}

	public class RentalTypeDto
	{
		public string Type { get; set; }
		public int Count { get; set; }
	}
}
