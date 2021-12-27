using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkLabLibrary.DataAccess;
using WorkLabWeb.Areas.WorkSpace.Models;
using WorkLabWeb.HubModels;

namespace WorkLabWeb.Areas.WorkSpace.Controllers
{
	public class SessionController : Controller
	{
		private readonly IWebHostEnvironment _env;

		public SessionController(IWebHostEnvironment env)
		{
			_env = env;
		}

		public IActionResult Dashboard()
		{
			return View();
		}

		public async Task<IActionResult> NewSession(string type)
		{
			var email = User.FindFirst(x => x.Type == ClaimTypes.Email).Value;

			var result = await EmailManager.IsEmailConfirmed(email).ConfigureAwait(false);

			if (!result)
				return RedirectToAction("Dashboard", "Session", new { Area = "WorkSpace" });

			return View("NewSession", type);
		}

		[AllowAnonymous]
		public IActionResult JoinSession()
		{
			var model = new JoinSessionViewModel();

			if (User.Identity.IsAuthenticated)
				model.UserName = User.Identity.Name;

			return View(model);
		}

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult JoinSession(JoinSessionViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			if (!SessionInformation.SessionInfo.ContainsKey(model.SessionKey))
			{
				ModelState.AddModelError("SessionKey", "Invalid session key");
				return View(model);
			}

			model.DocumentType = SessionInformation.SessionInfo[model.SessionKey].type;

			return View("_JoinSession", model);
		}

		[AllowAnonymous]
		[HttpPost]
		public List<string> UploadImageFile(List<IFormFile> imageFiles)
		{
			var imagePaths = new List<string>();

			var uploadFolder = Path.Combine(_env.WebRootPath, "assets", "document", "images");

			imageFiles.ForEach(file =>
			{
				var fileName = $"{Guid.NewGuid()}_{Path.GetRandomFileName()}_{file.FileName}";
				var filePath = Path.Combine(uploadFolder, fileName);
				var fileStream = new FileStream(filePath, FileMode.Create);
				file.CopyTo(fileStream);
				fileStream.Dispose();

				imagePaths.Add($"/assets/document/images/{fileName}");
			});

			return imagePaths;
		}
	}
}