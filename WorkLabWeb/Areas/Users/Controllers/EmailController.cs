using WorkLabLibrary.DataAccess;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkLabWeb.Services;
using WorkLabWeb.ServiceModels;
using Microsoft.Extensions.Configuration;

namespace WorkLabWeb.Areas.Users.Controllers
{
	public class EmailController : Controller
	{
		private readonly IConfiguration _configuration;

		private readonly IEmailService _emailService;

		public EmailController(IConfiguration configuration, IEmailService emailService)
		{
			_configuration = configuration;
			_emailService = emailService;
		}

		public async Task<IActionResult> VerifyEmail(string token)
		{
			await EmailManager.VerifyEmailAddress(token).ConfigureAwait(false);

			return RedirectToAction("Dashboard", "Session", new { Area = "WorkSpace" });
		}

		public async Task<IActionResult> SendAccountConfirmationEmail()
		{
			var email = User.FindFirst(x => x.Type == ClaimTypes.Email).Value;

			Guid token = await EmailManager.GetConfirmationToken(email).ConfigureAwait(false);

			var confirmationLink = Url.Action("VerifyEmail", "Email", new { Area = "Users", token }, Request.Scheme);

			var request = new EmailRequest
			{
				Email = email,
				Link = confirmationLink,
				Type = "confirmation",
				Secret = _configuration["EmailService:AudienceSecret"]
			};

			var response = await _emailService.SendEmail(request).ConfigureAwait(false);

			if (response)
				return Ok();

			return StatusCode(500);
		}
	}
}