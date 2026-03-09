using OpenAI;
using OpenAI.Chat;
using System.Data;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;


namespace Graduation_Project.Service
{
	public class ChatService
	{
		private readonly string _apiKey;
		private readonly HttpClient _http;
		
		public ChatService(IConfiguration config)
		{
			//_apiKey = config["OpenAI:ApiKey"];
			//_apiKey = config["Gemini:ApiKey"];
			_apiKey = config["Groq:ApiKey"];
			_http = new HttpClient();
		}


		//		public async Task<MedicalAIResponse> AnalyzeSymptoms(string symptoms)
		//		{
		//			var url = "https://api.openai.com/v1/chat/completions";

		//			var prompt = @"
		//You are a medical triage assistant.
		//You MUST return ONLY valid JSON.
		//NO explanations.
		//NO extra text.

		//JSON format:
		//{
		//  ""department"": ""string"",
		//  ""suggestedActions"": [""string""],
		//  ""howToUse"": ""string""
		//}
		//";

		//			var requestBody = new
		//			{
		//				model = "gpt-3.5-turbo",
		//				messages = new[]
		//				{
		//			new { role = "system", content = prompt },
		//			new { role = "user", content = symptoms }
		//		}
		//			};

		//			var content = new StringContent(
		//				JsonSerializer.Serialize(requestBody),
		//				Encoding.UTF8,
		//				"application/json"
		//			);

		//			_http.DefaultRequestHeaders.Authorization =
		//				new AuthenticationHeaderValue("Bearer", _apiKey);

		//			var response = await _http.PostAsync(url, content);
		//			var responseString = await response.Content.ReadAsStringAsync();

		//			// 👇 مهم
		//			if (!response.IsSuccessStatusCode)
		//				throw new Exception(responseString);

		//			using var doc = JsonDocument.Parse(responseString);

		//			var aiText =
		//				doc.RootElement
		//				   .GetProperty("choices")[0]
		//				   .GetProperty("message")
		//				   .GetProperty("content")
		//				   .GetString();

		//			return JsonSerializer.Deserialize<MedicalAIResponse>(
		//				aiText,
		//				new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
		//			);
		//		}



		//public async Task<MedicalAIResponse> AnalyzeSymptomsGemini(string symptoms)
		//{
		//	var url =
		//		$"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";

		//	var body = new
		//	{
		//		contents = new[]
		//		{
		//			new {
		//				parts = new[]
		//				{
		//					new { text =
		//@"Return ONLY JSON:
		//{
		// ""department"": ""string"",
		// ""suggestedActions"": [""string""],
		// ""howToUse"": ""string""
		//}

		//Symptoms: " + symptoms }
		//				}
		//			}
		//		}
		//	};

		//	var response = await _http.PostAsync(
		//		url,
		//		new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
		//	);

		//	var json = await response.Content.ReadAsStringAsync();

		//	using var doc = JsonDocument.Parse(json);
		//	var root = doc.RootElement;

		//	string aiText = null;

		//	if (root.TryGetProperty("candidates", out var candidates) &&
		//		candidates.GetArrayLength() > 0)
		//	{
		//		var candidate = candidates[0];

		//		if (candidate.TryGetProperty("content", out var content) &&
		//			content.TryGetProperty("parts", out var parts) &&
		//			parts.GetArrayLength() > 0 &&
		//			parts[0].TryGetProperty("text", out var textElement))
		//		{
		//			aiText = textElement.GetString();
		//		}
		//	}

		//	if (string.IsNullOrEmpty(aiText))
		//	{
		//		throw new Exception("Gemini response does not contain valid text.");
		//	}

		//	return JsonSerializer.Deserialize<MedicalAIResponse>(
		//		aiText,
		//		new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
		//	);

		//}


		//public async Task<MedicalAIResponse> AnalyzeSymptomsGroq(string symptoms)
		//{
		//	var url = "https://api.groq.com/openai/v1/chat/completions";

		//	var prompt = @"
		//You are a medical triage assistant.
		//Return ONLY valid JSON.
		//No explanation.
		//No extra text.

		//JSON format:
		//{
		//  ""department"": ""string"",
		//  ""suggestedActions"": [""string""],
		//  ""howToUse"": ""string""
		//}
		//";

		//	var requestBody = new
		//	{
		//		model = "llama-3.1-8b-instant",
		//		messages = new[]
		//		{
		//			new { role = "system", content = prompt },
		//			new { role = "user", content = symptoms }
		//		}
		//	};

		//	_http.DefaultRequestHeaders.Clear();
		//	_http.DefaultRequestHeaders.Authorization =
		//		new AuthenticationHeaderValue("Bearer", _apiKey);

		//	var response = await _http.PostAsync(
		//		url,
		//		new StringContent(
		//			JsonSerializer.Serialize(requestBody),
		//			Encoding.UTF8,
		//			"application/json")
		//	);

		//	var responseText = await response.Content.ReadAsStringAsync();

		//	if (!response.IsSuccessStatusCode)
		//		throw new Exception(responseText);

		//	using var doc = JsonDocument.Parse(responseText);

		//	var aiText =
		//		doc.RootElement
		//		   .GetProperty("choices")[0]
		//		   .GetProperty("message")
		//		   .GetProperty("content")
		//		   .GetString();

		//	return JsonSerializer.Deserialize<MedicalAIResponse>(
		//		aiText,
		//		new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
		//	);
		//}




		public async Task<MedicalAIResponse> AnalyzeSymptomsGroq(
	string message,
	List<string> allowedDepartments)
		{
			var url = "https://api.groq.com/openai/v1/chat/completions";

			var departmentsText = string.Join(",", allowedDepartments);

			var prompt = $@"
You are a STRICT medical classification assistant.

Your ONLY task:
Classify the patient's symptoms into EXACTLY ONE department 
from the allowed list below.

=====================
ALLOWED DEPARTMENTS:
{departmentsText}
=====================

CRITICAL RULES:

1) Pediatrics:
- Choose ONLY if the patient is explicitly a child
- Or age under 12 is clearly mentioned
- NEVER choose Pediatrics for adults

2) Dermatology:
- Skin problems
- Rash
- Acne
- Itching
- Skin infection
- Burn

3) Cardiology:
- Chest pain
- Heart pain
- Palpitations
- High blood pressure

4) Neurology:
- Headache
- Seizures
- Numbness
- Stroke symptoms

5) Neurosurgery:
- Brain tumor
- Spinal surgery cases

6) Orthopedics:
- Bone pain
- Joint pain
- Fracture

7) Ophthalmology:
- Eye pain
- Vision problems
- Red eye

8) ENT:
- Ear pain
- Throat pain
- Sinus issues

9) Obstetrics & Gynecology:
- Pregnancy
- Menstrual issues
- Female reproductive problems

10) Oncology:
- Cancer related symptoms
- Tumors

11) Psychiatric:
- Anxiety
- Depression
- Panic attacks

12) Emergency:
- Severe bleeding
- Loss of consciousness
- Serious trauma

13) General Medicine:
- Fever
- General weakness
- Non-specific symptoms

14) Chest Diseases / Pulmonology:
- Cough
- Shortness of breath
- Lung problems

15) Infectious / Contagious Diseases:
- Viral infection
- Bacterial infection
- Epidemic disease

IMPORTANT:
- NEVER guess randomly.
- If symptoms match multiple departments,
  choose the MOST medically specific one.
- If no strong match, choose General Medicine.

Return ONLY valid JSON:
{{
 ""department"": ""string"",
 ""city"": ""string"",
 ""suggestedActions"": [""string""],
 ""howToUse"": ""string""
}}
";
			

			var requestBody = new
			{
				model = "llama-3.3-70b-versatile",
				messages = new[]
				{
			new { role="system", content=prompt },
			new { role="user", content=message }
		}
			};

			_http.DefaultRequestHeaders.Clear();
			_http.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", _apiKey);

			var response = await _http.PostAsync(
				url,
				new StringContent(
					JsonSerializer.Serialize(requestBody),
					Encoding.UTF8,
					"application/json"));

			var responseText = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				throw new Exception(responseText);


			using var doc = JsonDocument.Parse(responseText);

			//var aiText =
			//	doc.RootElement
			//	   .GetProperty("choices")[0]
			//	   .GetProperty("message")
			//	   .GetProperty("content")
			//	   .GetString();

			//return JsonSerializer.Deserialize<MedicalAIResponse>(
			//	aiText,
			//	new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			//);
			var aiText =
	doc.RootElement
	   .GetProperty("choices")[0]
	   .GetProperty("message")
	   .GetProperty("content")
	   .GetString();

			if (string.IsNullOrWhiteSpace(aiText))
			{
				throw new Exception("AI returned empty response.");
			}

			// 🔥 تنظيف الرد لو فيه نص قبل أو بعد JSON
			var jsonStart = aiText.IndexOf("{");
			var jsonEnd = aiText.LastIndexOf("}");

			if (jsonStart == -1 || jsonEnd == -1)
			{
				throw new Exception("AI did not return valid JSON: " + aiText);
			}

			var cleanJson = aiText.Substring(jsonStart, jsonEnd - jsonStart + 1);

			try
			{
				return JsonSerializer.Deserialize<MedicalAIResponse>(
					cleanJson,
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
				);
			}
			catch
			{
				throw new Exception("Failed to parse AI JSON: " + cleanJson);
			}

		}




	}

	public class MedicalAIResponse
	{
		public string Department { get; set; }
		public string? City { get; set; }
		public List<string> SuggestedActions { get; set; }
		public string HowToUse { get; set; }
	}


}
