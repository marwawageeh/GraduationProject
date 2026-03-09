namespace Graduation_Project.DTO
{
	public class VerifyBookingPaymentDto
	{
		public string PaymentIntentId { get; set; }
		public string PatientName { get; set; }
		public string PatientEmail { get; set; }
		public string PatientPhone { get; set; }
		public string BookingType { get; set; }
	}
}
