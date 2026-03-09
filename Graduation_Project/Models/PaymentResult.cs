namespace Graduation_Project.Models
{
	public class PaymentResult
	{
		public string PaymentIntentId { get; set; }
		public string ClientSecret { get; set; }
		public string PaymentUrl { get; set; }
	}
}
