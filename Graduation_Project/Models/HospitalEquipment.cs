namespace Graduation_Project.Models
{
	public class HospitalEquipment
	{
		public int HospitalEquipmentId { get; set; }
		public string Name { get; set; }

		public ICollection<Hospital> Hospitals { get; set; }
	}
}
