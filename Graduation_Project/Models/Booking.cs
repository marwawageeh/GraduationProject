using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public DateTime BookingDate { get; set; }
		public TimeSpan BookingTime { get; set; }

		public decimal Price { get; set; }

		public string Status { get; set; } = "Pending";// Pending, Confirmed, Completed, Canceled
		public string? Notes { get; set; }
		public string? PatientName { get; set; }
		public string? PatientEmail { get; set; }
		public string? PatientPhone { get; set; }
        public string? BookingTypes {  get; set; }


		public string? PaymentIntentId { get; set; }

		public Payment? Payment { get; set; }
    }
}
