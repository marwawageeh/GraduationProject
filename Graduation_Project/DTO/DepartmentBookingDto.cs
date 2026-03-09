namespace Graduation_Project.DTO
{
	public class DepartmentBookingDto
	{
		public int DepartmentId { get; set; }
		public string Name { get; set; }
		public List<DoctorBookingDto> Doctors { get; set; }
	}
}
