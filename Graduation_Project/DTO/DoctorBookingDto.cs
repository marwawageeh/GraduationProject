namespace Graduation_Project.DTO
{
	public class DoctorBookingDto
	{
		public int DoctorId { get; set; }
		public string Name { get; set; }
		public string Specialization { get; set; }
		public int ExperienceYears { get; set; }
		public string ImageURL { get; set; }

		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public string Avaliable {  get; set; }
	}
}
