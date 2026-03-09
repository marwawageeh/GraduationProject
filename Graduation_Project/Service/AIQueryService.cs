using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Graduation_Project.Service
{
	public class AIQueryService
	{
		private readonly HttpClient _http;
		private readonly string _groqKey;
		private readonly string _geminiKey;

		public AIQueryService(IConfiguration config)
		{
			_http = new HttpClient();
			_groqKey = config["Groq:ApiKey"];
			_geminiKey = config["Gemini:ApiKey"];
		}

		// 🔥 الميثود الوحيدة اللي الكنترولر يناديها
		public async Task<string?> GetAIResponse(string prompt)
		{
			// 1️⃣ Groq
			var groq = await CallGroq(prompt);
			if (!string.IsNullOrWhiteSpace(groq))
				return groq;

			// 2️⃣ Gemini
			var gemini = await CallGemini(prompt);
			if (!string.IsNullOrWhiteSpace(gemini))
				return gemini;

			// 3️⃣ Fallback
			return null;
		}

		// ================= GROQ =================
		private async Task<string?> CallGroq(string prompt)
		{
			try
			{
				var body = new
				{
					model = "llama-3.1-8b-instant",
					messages = new[]
					{
						new { role = "system", content = "Return ONLY valid JSON. No markdown." },
						new { role = "user", content = prompt }
					},
					temperature = 0
				};

				_http.DefaultRequestHeaders.Clear();
				_http.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Bearer", _groqKey);

				var res = await _http.PostAsync(
					"https://api.groq.com/openai/v1/chat/completions",
					new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
				);

				if (!res.IsSuccessStatusCode)
					return null;

				var json = await res.Content.ReadAsStringAsync();
				using var doc = JsonDocument.Parse(json);

				return doc.RootElement
					.GetProperty("choices")[0]
					.GetProperty("message")
					.GetProperty("content")
					.GetString();
			}
			catch
			{
				return null;
			}
		}

		// ================= GEMINI =================
		private async Task<string?> CallGemini(string prompt)
		{
			try
			{
				var body = new
				{
					contents = new[]
					{
						new
						{
							parts = new[]
							{
								new { text = prompt }
							}
						}
					}
				};

				var res = await _http.PostAsync(
					$"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_geminiKey}",
					new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
				);

				if (!res.IsSuccessStatusCode)
					return null;

				var json = await res.Content.ReadAsStringAsync();
				using var doc = JsonDocument.Parse(json);

				return doc.RootElement
					.GetProperty("candidates")[0]
					.GetProperty("content")
					.GetProperty("parts")[0]
					.GetProperty("text")
					.GetString();
			}
			catch
			{
				return null;
			}
		}
	}

	public class SearchQueryResult
	{
		public string Name { get; set; }
		public string Location { get; set; } = "";
		public double? MinRating { get; set; }
		public string Device { get; set; } = "";
		public int? MaxPrice { get; set; }
		public string Department { get; set; } = "";
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
	}

}
