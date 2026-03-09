namespace Graduation_Project.DTO
{
	public class DoctorBookingDtoDashboard
	{
		public int BookingId { get; set; }
		public string PatientName { get; set; }
		public string PatientPhone { get; set; }
		public string PatientEmail { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan Time { get; set; }
		public string Type { get; set; }
		public string Status { get; set; }
	}
}
