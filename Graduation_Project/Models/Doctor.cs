using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public double Rating { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
