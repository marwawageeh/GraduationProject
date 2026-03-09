namespace Graduation_Project.DTO
{
	public class ProfileResponseDto
	{
		public int UserId { get; set; }
		public string Name { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public string ImageUrl { get; set; }
	}
}
