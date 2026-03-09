using Graduation_Project.Context;
using Graduation_Project.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ContactUsController : ControllerBase
	{
		private readonly AppDBContext _context;

        public ContactUsController(AppDBContext DB)
        {
			_context = DB;
		}


		[HttpPost]
		public IActionResult SendMessage(Message msg)
		{
			try
			{
				var smtp = new SmtpClient("smtp.gmail.com", 587)
				{
					Credentials = new NetworkCredential(
						"Marwawageeh75@gmail.com",
						"oybt moii uroa yght"
					),
					EnableSsl = true
				};

				var mail = new MailMessage();
				mail.From = new MailAddress(msg.email);
				mail.To.Add("Marwawageeh75@gmail.com");
				mail.Subject = msg.Subject;
				mail.Body = $"Name: {msg.name}\nEmail: {msg.email}\nMessage: {msg.Body}";

				smtp.Send(mail);

				return Ok("Message Sent Successfully");
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}


	}
}
