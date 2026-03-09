namespace Graduation_Project.DTO
{
	public class AdminOperationDto
	{
		public int Id { get; set; }
		public string CustomerName { get; set; }
		public string Type { get; set; } // كشف أو تأجير
		public string ItemName { get; set; } // دكتور أو جهاز
		public DateTime Date { get; set; }
		public string Time { get; set; }
		public decimal Amount { get; set; }
		public string Status { get; set; }
	}
}
