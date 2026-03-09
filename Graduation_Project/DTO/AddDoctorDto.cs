namespace Graduation_Project.DTO
{
	public class AddDoctorDto
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Specialization { get; set; }
		public int ExperienceYears { get; set; }
		public decimal ConsultationPrice { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public int DepartmentId { get; set; }
		public IFormFile Image { get; set; }
	}
}
