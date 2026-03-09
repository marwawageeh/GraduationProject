namespace Graduation_Project.Models
{
	public class DeliveryAddress
	{
		public int DeliveryAddressId { get; set; }

		public int RentalId { get; set; }
		public Rental Rental { get; set; }

		public string FullName { get; set; }
		public string Phone { get; set; }
		public string StreetAddress { get; set; }
		public string Apartment { get; set; }
		public string City { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
