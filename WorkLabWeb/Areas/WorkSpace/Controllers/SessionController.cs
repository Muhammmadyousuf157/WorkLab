using Aspose.Html;
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
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using WorkLabLibrary.Models;

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

		public async Task<IActionResult> NewSession(string type, string filePath, int sessionId, string fileTitle)
		{
			if (filePath is not null)
			{
				await SessionManager.DeleteSession(sessionId).ConfigureAwait(false);

				var path = Path.Combine(_env.WebRootPath, "assets","session","files", filePath);
				ViewBag.FileContent = await CodeFile.ReadAllTextAsync(path);

				ViewBag.FileTitle = fileTitle;
			}

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


		[HttpPost]
		public async Task<IActionResult> UpdateFileTitle(int fileId, string fileTitle)
		{
			await SessionManager.UpdateFileTitle(fileId, fileTitle);

			return Ok();
		}

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
		public async Task<string> GetSessionFileTitle(string sessionKey)
		{
			var title =  await SessionManager.GetSessionFileTitle(sessionKey);
			return title;
		}

		[AllowAnonymous]
		public async Task<IActionResult> GetSessionFileId(string sessionKey)
		{
			var fileId = await SessionManager.GetSessionFileId(sessionKey);
			return Ok(fileId);
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

		   [HttpPost]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            await SessionManager.DeleteSession(sessionId).ConfigureAwait(false);

            return Ok();
        }

		[AllowAnonymous]
		public async Task<IActionResult> DownloadDoc(int fileId)
		{
			var sessionFile = await SessionManager.GetSessionFile(fileId).ConfigureAwait(false);

			var txtFile = Path.Combine(_env.WebRootPath, "assets", "session", "files", sessionFile.FilePath);
			var htmlFile = Path.Combine(_env.WebRootPath, "assets", "session", "temp", $"{Guid.NewGuid()}_{Path.GetRandomFileName()}.html");
			var docFile = Path.Combine(_env.WebRootPath, "assets", "session", "temp", $"{Guid.NewGuid()}_{Path.GetRandomFileName()}.docx");

			await CodeFile.Create(htmlFile).DisposeAsync();
			await CodeFile.WriteAllTextAsync(htmlFile, await CodeFile.ReadAllTextAsync(txtFile));

			// Initialize an HTML document from the file
			using var document = new HTMLDocument(htmlFile);

			// Initialize DocSaveOptions 
			var options = new Aspose.Html.Saving.DocSaveOptions();

			// Convert HTML webpage to DOCX
			Aspose.Html.Converters.Converter.ConvertHTML(document, options, docFile);

			var memoryStream = new MemoryStream();

			using var fileStream = new FileStream(docFile, FileMode.Open);

			await fileStream.CopyToAsync(memoryStream);

			memoryStream.Position = 0;

			return File(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{sessionFile.FileTitle}.docx");
		}


		//[AllowAnonymous]
		//public async Task<IActionResult> DownloadDoc(int fileId)
		//{
		//	var sessionFile = await SessionManager.GetSessionFile(fileId).ConfigureAwait(false);

		//	var txtFile = Path.Combine(_env.WebRootPath, "assets", "session", "files", sessionFile.FilePath);
		//	var htmlFile = Path.Combine(_env.WebRootPath, "assets", "session", "temp", $"{Guid.NewGuid()}_{Path.GetRandomFileName()}.html");
		//	var docFile = Path.Combine(_env.WebRootPath, "assets", "session", "temp", $"{Guid.NewGuid()}_{Path.GetRandomFileName()}.docx");

		//	await CodeFile.Create(htmlFile).DisposeAsync();
		//	await CodeFile.WriteAllTextAsync(htmlFile, await CodeFile.ReadAllTextAsync(txtFile));

		//	using (WordDocument document = new WordDocument(htmlFile, FormatType.Html, XHTMLValidationType.None))

		//		document.Save(docFile, FormatType.Docx);

		//	var memoryStream = new MemoryStream();

		//	using var fileStream = new FileStream(docFile, FileMode.Open);

		//	await fileStream.CopyToAsync(memoryStream);

		//	memoryStream.Position = 0;

		//	return File(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{sessionFile.FileTitle}.docx");
		//}

		public async Task<IActionResult> DownloadSpreadSheet(int fileId) 
		{
			var file = await SessionManager.GetSessionFile(fileId);
			var path = Path.Combine(_env.WebRootPath, "assets", "session", "files", file.FilePath);
			var spreadsheetFile = new SpreadSheetFile
			{
				Title = file.FileTitle,
				Content = await CodeFile.ReadAllTextAsync(path)
			};

			return Ok(new { File = spreadsheetFile});
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

				imagePaths.Add($"https://worklab.azurewebsites.net/assets/document/images/{fileName}");
			});

			return imagePaths;
		}


	}
}