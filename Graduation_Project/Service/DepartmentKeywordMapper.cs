namespace Graduation_Project.Service
{
	public static class DepartmentKeywordMapper
	{
		private static readonly Dictionary<string, string> ArabicToEnglish = new()
	{
		{ "اطفال", "Pediatrics" },
		{ "قلب", "Cardiology" },
		{ "اورام", "Oncology" },
		{ "عظام", "Orthopedics" },
		{ "انف واذن", "ENT" },
		{ "عيون", "Ophthalmology" },
		{ "اعصاب", "Neurology" },
		{ "مخ واعصاب", "Neurology & Neurosurgery" },
		{ "باطنه", "Internal Medicine" },
		{ "طوارئ", "Emergency" },
		{ "جراحه", "Surgery" },
		{ "صدر", "Chest Diseases" },
		{ "نفسيه", "Psychiatric" }
	};

		public static string? DetectDepartment(string text)
		{
			foreach (var item in ArabicToEnglish)
			{
				if (text.Contains(item.Key))
					return item.Value;
			}

			return null;
		}
	}
}
