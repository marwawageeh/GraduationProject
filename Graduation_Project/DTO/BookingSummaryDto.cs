namespace Graduation_Project.DTO
{
	public class BookingSummaryDto
	{
		public int BookingId { get; set; }

		public string DoctorName { get; set; }
		public string DepartmentName { get; set; }
		public string HospitalName { get; set; }

		public DateTime Date { get; set; }
		public TimeSpan Time { get; set; }

		public decimal Price { get; set; }
	}
}
