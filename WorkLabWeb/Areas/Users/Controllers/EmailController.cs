using WorkLabLibrary.DataAccess;
using FluentEmail.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WorkLabWeb.Areas.Users.Controllers
{
	public class EmailController : Controller
	{
		private readonly IFluentEmail _email;

		private readonly IWebHostEnvironment _env;

		public EmailController(IFluentEmail email, IWebHostEnvironment env)
		{
			_email = email;
			_env = env;
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

			var templateFilePath = $"{_env.ContentRootPath}\\EmailTemplates\\EmailConfirmation.cshtml";

			try
			{
				await _email
			   .To(email, User.Identity.Name)
			   .Subject("Email Confirmation - WorkLab")
			   .UsingTemplateFromFile(templateFilePath, new { Link = confirmationLink }, true)
			   .SendAsync()
			   .ConfigureAwait(false);
			}
			catch (Exception ex)
			{
			}
		   
			return Ok();
		}
	}
}