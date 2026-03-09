namespace Graduation_Project.DTO
{
	public class CreateBookingDto
	{
		public int DoctorId { get; set; }
		public DateTime Date { get; set; }
		public string Time { get; set; } // "12:00"

		public string Notes { get; set; }
		//public decimal Price { get; set; }
	}
}
