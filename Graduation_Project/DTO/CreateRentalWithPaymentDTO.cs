namespace Graduation_Project.DTO
{
	public class CreateRentalWithPaymentDTO
	{
		public int EquipmentId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public string FullName { get; set; }
		public string Phone { get; set; }
		public string StreetAddress { get; set; }
		public string Apartment { get; set; }
		public string City { get; set; }
	}
}
