using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public double Rating { get; set; }
        public string Description {  get; set; }

        public DateTime Start_Time { get; set; }

        public DateTime End_Time { get; set; }

        public ICollection<Department> Departments { get; set; }
    }
}
