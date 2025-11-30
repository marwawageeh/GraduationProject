using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Graduation_Project.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? Role { get; set; } // Admin, Doctor, Patient
        public string Phone { get; set; }
        public string? Location { get; set; }

        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Rental> Rentals { get; set; }
        public ICollection<ChatHistory> ChatHistories { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
