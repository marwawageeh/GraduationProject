using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Rental
    {
        public int RentalId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Equipment")]
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }

        public Payment Payment { get; set; }
    }
}
