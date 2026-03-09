using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
	public class AddEquipmentDto
	{
		[Required]
		public string Name { get; set; }

		[Required]
		public string Description { get; set; }

		[Range(1, 100000)]
		public decimal PricePerDay { get; set; }
		public IFormFile Image { get; set; }
	}
}
