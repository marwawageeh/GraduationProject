using System.Net.Mail;
using System.Net;

namespace Graduation_Project.Service
{
	public class EmailService
	{
		public void SendEmail(string to, string subject, string body)
		{
			var smtp = new SmtpClient("smtp.gmail.com", 587)
			{
				Credentials = new NetworkCredential("Marwawageeh75@gmail.com", "oybt moii uroa yght"),
				EnableSsl = true
			};

			smtp.Send("Marwawageeh75@gmail.com", to, subject, body);
		}
	}
}
