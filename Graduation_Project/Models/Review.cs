namespace Graduation_Project.Models
{
	public class Review
	{
		public int ReviewId { get; set; }

		public int EquipmentId { get; set; }
		public Equipment Equipment { get; set; }

		public string UserName { get; set; }

		public int Rating { get; set; } // 1 to 5
		public string Comment { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
