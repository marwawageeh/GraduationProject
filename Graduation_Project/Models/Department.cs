using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("Hospital")]
        public int HospitalId { get; set; }
        public Hospital Hospital { get; set; }

        public ICollection<Doctor> Doctors { get; set; }
    }
}
