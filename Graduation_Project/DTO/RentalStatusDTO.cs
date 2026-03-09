namespace Graduation_Project.DTO
{
	public class RentalStatusDTO
	{
		public int RentalId { get; set; }
		public double RemainingHours { get; set; }
		public bool IsExpired { get; set; }
		public bool IsEndingSoon { get; set; }
	}
}
