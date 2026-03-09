namespace Graduation_Project.Models
{
	public class PendingUser
	{
		public int Id { get; set; }

		public string Name { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public string Phone { get; set; }

		public string Code { get; set; }
		public DateTime ExpireAt { get; set; }
	}
}
