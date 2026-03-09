using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public bool Availability { get; set; }

        [Precision(18, 2)]
        public decimal PricePerDay { get; set; }
		public string? ImageUrl { get; set; }
        public bool IsUnderMaintenance { get; set; }


		[ForeignKey("Owner")]
        public int? OwnerId { get; set; }
		public User? Owner { get; set; }

		public ICollection<Rental> Rentals { get; set; }
    }
}
