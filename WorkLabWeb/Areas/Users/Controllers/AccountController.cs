using AutoMapper;
using WorkLabLibrary.DataAccess;
using WorkLabLibrary.Models;
using WorkLabWeb.Areas.Users.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentEmail.Core;
using Microsoft.AspNetCore.Hosting;

namespace WorkLabWeb.Areas.Users.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IFluentEmail _email;
        private readonly IWebHostEnvironment _env;

        public AccountController(IMapper mapper, IFluentEmail email, IWebHostEnvironment env)
        {
            _mapper = mapper;
            _email = email;
            _env = env;
        }

        public IActionResult SignIn()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        public async Task<IActionResult> ValidateEmailAddress(string emailAddress)
        {
            var result = await AccountManager.IsUniqueEmailAddress(emailAddress)
                .ConfigureAwait(false);

            return Json(result);
        }

        public async Task<IActionResult> ValidateUserName(string userName)
        {
            var result = await AccountManager.IsUniqueUserName(userName)
                .ConfigureAwait(false);

            return Json(result);
        }

        private async Task<ActionResult> AuthorizeUser(User user, string url)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.GivenName, user.UserName),
                new(ClaimTypes.Email, user.EmailAddress)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity))
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(url))
                return LocalRedirect(url);

            return RedirectToAction("Dashboard", "Session", new { Area = "WorkSpace" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userModel = await AccountManager.UserSignIn(model.EmailAddress, model.Password)
                .ConfigureAwait(false);

            if (userModel is null)
            {
                ModelState.AddModelError("SignInFailed", "Incorrect email address or password");
                return View(model);
            }

            return await AuthorizeUser(userModel, returnUrl).ConfigureAwait(false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userModel = _mapper.Map<User>(model);

            var response = await AccountManager.UserSignUp(userModel)
                .ConfigureAwait(false);

            if (response == -1)
            {
                ModelState.AddModelError("UserName", "User name is already taken");
                return View(model);
            }

            if (response == -2)
            {
                ModelState.AddModelError("EmailAddress", "Email address is already taken");
                return View(model);
            }

            return await AuthorizeUser(userModel, null).ConfigureAwait(false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme)
                .ConfigureAwait(false);

            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = await AccountManager.GetForgotPasswordToken(model.EmailAddress)
                .ConfigureAwait(false);

            if (token != null)
            {
                var passwordResetLink = Url.Action("ResetPassword", "Account", new { Area = "Users", token }, Request.Scheme);

                var templateFilePath = $"{_env.ContentRootPath}\\EmailTemplates\\PasswordReset.cshtml";

                await _email
                    .To(model.EmailAddress)
                    .Subject("Password Reset - WORKLAB")
                    .UsingTemplateFromFile(templateFilePath, new { Link = passwordResetLink }, true)
                    .SendAsync()
                    .ConfigureAwait(false);
            }

            return View("ForgotPasswordResponse");
        }

        public IActionResult ResetPassword(string token)
        {
            var model = new ResetPasswordViewModel { Token = token };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await AccountManager.ResetPassword(model.Password, model.Token).ConfigureAwait(false);

            return RedirectToAction("SignIn", "Account", new { Area = "Users" });
        }
    }
}