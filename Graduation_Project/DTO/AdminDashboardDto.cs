namespace Graduation_Project.DTO
{
	public class AdminDashboardDto
	{
		public int TotalUsers { get; set; }
		public int TotalDoctors { get; set; }
		public int TotalBookings { get; set; }
		public int TotalRental { get; set; }
		public int TotalHospitals { get; set; }
		public int TotalEquipments { get; set; }
		public decimal TotalRevenue { get; set; }

		public List<string> MonthNames { get; set; }
		public List<int> MonthlyBookings { get; set; }
		public List<int> MonthlyRentals { get; set; }

		public List<string> WeeklyDays { get; set; }
		public List<int> WeeklyBookings { get; set; }

		public List<string> BookingTypesNames { get; set; }
		public List<int> BookingTypesCount { get; set; }

	}
}
