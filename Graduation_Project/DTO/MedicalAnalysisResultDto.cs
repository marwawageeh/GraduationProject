namespace Graduation_Project.DTO
{
	public class MedicalAnalysisResultDto
	{
		public string Department { get; set; }
		public List<string> SuggestedActions { get; set; }
		public string HowToUse { get; set; }
		public List<NearbyHospitalDto> Hospitals { get; set; }
	}
}
