using Graduation_Project.Models;

namespace Graduation_Project.DTO
{
	public class HospitalBookingDetailsDto
	{
		public int HospitalId { get; set; }
		public string HospitalName { get; set; }
		public List<DepartmentBookingDto> Departments { get; set; }


	}
}
