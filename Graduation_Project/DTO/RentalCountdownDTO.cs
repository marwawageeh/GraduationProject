namespace Graduation_Project.DTO
{
	public class RentalCountdownDTO
	{
		public int RentalId { get; set; }
		public double RemainingHours { get; set; }
		public double RemainingDays { get; set; }
		public bool IsExpired { get; set; }
	}
}
