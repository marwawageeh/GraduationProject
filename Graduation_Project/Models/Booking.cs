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
        public string Status { get; set; } // Pending, Confirmed, Completed, Canceled
        public string Notes { get; set; }

        public Payment Payment { get; set; }
    }
}
