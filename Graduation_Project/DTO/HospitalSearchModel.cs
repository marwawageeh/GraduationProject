namespace Graduation_Project.DTO
{
	public class HospitalSearchModel
	{
		public int? DepartmentId { get; set; }
		public decimal? MaxPrice { get; set; }
		public double? MinRating { get; set; }
		public List<int>? EquipmentIds { get; set; }
		public double? UserLat { get; set; }
		public double? UserLng { get; set; }
		public string? SearchText { get; set; }

	}
}
