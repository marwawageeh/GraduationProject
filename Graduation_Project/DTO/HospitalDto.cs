using Graduation_Project.Models;

namespace Graduation_Project.DTO
{
	public class HospitalDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		//public double? Rating { get; set; }
		public double? Average { get; set; }
        public double? Count { get; set; }
		public decimal? ReservationPrice { get; set; }
		public string? ImageUrl { get; set; }
		public string Description { get; set; }
		public List<string> Departments { get; set; }
		public List<string> Equipments { get; set; }
		public string? Direction {  get; set; }

	}
}
