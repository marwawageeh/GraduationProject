using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Graduation_Project.Service
{
	public class SearchQueryParserService
	{
		private readonly AIQueryService _ai;

		public SearchQueryParserService(AIQueryService ai)
		{
			_ai = ai;
		}

		//		public async Task<SearchQueryResult> Parse(string text)
		//		{
		//			var prompt = $@"
		//حلل النص التالي واستخرج الشروط بدقة شديدة.

		//قواعد صارمة:
		//- لو المستخدم قال (مش أقل من 4.5) أو (على الأقل 4.5) → minRating = 4.5
		//- ممنوع التقريب
		//- ممنوع تخمين
		//- لو الشرط غير موجود خليه null

		//أرجع JSON فقط بدون أي شرح.

		//Schema:
		//{{
		//  ""location"": string | null,
		//  ""minRating"": number | null,
		//  ""department"": string | null,
		//  ""device"": string | null,
		//  ""maxPrice"": number | null
		//}}

		//النص:
		//{text}
		//";
		public async Task<SearchQueryResult> Parse(string text)
		{
			var prompt = $@"
You are an advanced hospital search parser.

Understand Arabic and English.

Extract structured filters from the user query.

Rules:
- Do NOT invent values.
- If something is not clearly mentioned → return null.
- Convert Arabic to English if needed.
- Be intelligent with synonyms.

Examples:
'مستشفى قلب في طنطا تقييم 4.5'
→ department: Cardiology
→ location: Tanta
→ minRating: 4.5

'mri in cairo cheap'
→ device: MRI
→ location: Cairo

Return ONLY valid JSON.
No explanation.
No markdown.

Schema:
{{
  ""Latitude"": double | null,
  ""Longitude"": double | null,
  ""Name"": string | null,
  ""location"": string | null,
  ""minRating"": number | null,
  ""department"": string | null,
  ""device"": string | null,
  ""maxPrice"": number | null
}}

User text:
{text}
";


		var aiResult = await _ai.GetAIResponse(prompt);

			if (string.IsNullOrWhiteSpace(aiResult))
				return new SearchQueryResult();

			try
			{
				var result = JsonSerializer.Deserialize<SearchQueryResult>(
					aiResult,
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
				) ?? new SearchQueryResult();

				return result;
			}
			catch
			{
				return new SearchQueryResult();
			}
		}

	}
}
