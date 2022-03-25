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
using CodeFile = System.IO.File;

namespace WorkLabWeb.Areas.WorkSpace.Controllers
{
	public class SessionController : Controller
	{
		private readonly IWebHostEnvironment _env;

		public SessionController(IWebHostEnvironment env)
		{
			_env = env;
		}

		public async Task<IActionResult> Dashboard()
		{
			var email = User.FindFirst(x => x.Type == ClaimTypes.Email).Value;

			var sessions = await SessionManager.GetSessions(email).ConfigureAwait(false);

			return View(sessions);
		}

		public async Task<IActionResult> NewSession(string type)
		{
			var email = User.FindFirst(x => x.Type == ClaimTypes.Email).Value;

			var result = await EmailManager.IsEmailConfirmed(email).ConfigureAwait(false);

			if (!result)
				return RedirectToAction("Dashboard", "Session", new { Area = "WorkSpace" });

			ViewBag.IsSession = true;
			ViewBag.NewSession = true;

			return View("NewSession", type);
		}

		[HttpPost]
		public async Task<IActionResult> SaveSession(string startDateTime, string sessionKey, string type)
		{
			var fileName = $"{Guid.NewGuid()}_{Path.GetRandomFileName()}.txt";

			var filePath = Path.Combine(_env.WebRootPath, "assets", "session", "files", fileName);

			await CodeFile.Create(filePath).DisposeAsync();

			var email = User.FindFirst(x => x.Type == ClaimTypes.Email).Value;

			return Ok(await SessionManager.SaveSession(email, startDateTime, sessionKey, fileName, type).ConfigureAwait(false));
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> SaveParticipant(string userName, string sessionKey)
		{
			await SessionManager.SaveParticipant(userName, sessionKey).ConfigureAwait(false);

			return Ok();
		}

		//[HttpPost]
		//public async Task<IActionResult> DeleteSession(int sessionId)
		//{
		//	await SessionManager.DeleteSession(sessionId).ConfigureAwait(false);

		//	return Ok();
		//}

		[HttpPost]
		public async Task<IActionResult> UpdateFileContent([FromQuery] string filePath, [FromQuery] string sessionKey, [FromBody] string fileContent)
		{
			var path = Path.Combine(_env.WebRootPath, "assets", "session", "files", filePath);

			if (!CodeFile.Exists(path))
				return BadRequest();

			SessionInformation.SessionInfo[sessionKey].fileContent.Clear();
			SessionInformation.SessionInfo[sessionKey].fileContent.Append(fileContent);

			await CodeFile.WriteAllTextAsync(path, fileContent).ConfigureAwait(false);

			return Ok();
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

			ViewBag.IsSession = true;
			ViewBag.JoinSession = true;

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