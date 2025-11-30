using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; }
        public string Method { get; set; } // e.g. PayPal, Card, Cash
        public string Status { get; set; }

        [ForeignKey("Booking")]
        public int? BookingId { get; set; }
        public Booking Booking { get; set; }

        [ForeignKey("Rental")]
        public int? RentalId { get; set; }
        public Rental Rental { get; set; }
    }
}
