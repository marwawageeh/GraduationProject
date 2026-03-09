using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Models
{
	public class HospitalReview
	{
		public int HospitalReviewId { get; set; }

		public int HospitalId { get; set; }
		public Hospital Hospital { get; set; }

		public string UserName { get; set; }

		public int Rating { get; set; } // 1 to 5
		public string? Comment { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
