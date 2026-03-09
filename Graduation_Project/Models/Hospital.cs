using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Hospital
    {
        public int HospitalId { get; set; }

        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description {  get; set; }
		
        [NotMapped]
		public double? Distance { get; set; }

		public DateTime Start_Time { get; set; }

        public DateTime End_Time { get; set; }
		public decimal? ReservationPrice { get; set; }
        public string? ImageUrl { get; set; }   
        public int? PhoneNumber { get; set; }

		public ICollection<Department> Departments { get; set; }
		public ICollection<HospitalEquipment> Equipments { get; set; }
        public ICollection<HospitalReview> Reviews { get; set; }
	}
}
