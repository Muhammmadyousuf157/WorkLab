using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkLabLibrary.DataAccess;
using WorkLabWeb.Areas.WorkSpace.Models;

namespace WorkLabWeb.Areas.WorkSpace.Controllers
{
	public class SessionController : Controller
	{
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
	}
}