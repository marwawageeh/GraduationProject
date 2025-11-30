using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

		public ICollection<Rental> Rentals { get; set; }
    }
}
