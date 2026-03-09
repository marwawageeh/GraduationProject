using System.Text.RegularExpressions;
using System.Text;

namespace Graduation_Project.Service
{
	public static class TextNormalizer
	{
		public static string Normalize(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return string.Empty;

			text = text.ToLower();

			// إزالة التشكيل
			text = Regex.Replace(text, @"[\u064B-\u065F]", "");

			// توحيد الحروف
			text = text.Replace("أ", "ا")
					   .Replace("إ", "ا")
					   .Replace("آ", "ا")
					   .Replace("ة", "ه")
					   .Replace("ى", "ي");

			return text;
		}
	}
}
