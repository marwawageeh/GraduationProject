namespace Graduation_Project.DTO
{
	public class AdminAddEquipmentDto
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal PricePerDay { get; set; }
		public IFormFile? Image { get; set; }

		// 👇 بيانات المالك
		public string OwnerName { get; set; }
		public string OwnerEmail { get; set; }
		public string OwnerPassword { get; set; }
		public string OwnerPhone { get; set; }
	}
}
