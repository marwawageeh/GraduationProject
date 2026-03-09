using Graduation_Project.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.DTO
{
	public class BookRequest
	{
		public int DoctorId { get; set; }
		public DateTime BookingDate { get; set; }
		public string Notes { get; set; }


	}
}
