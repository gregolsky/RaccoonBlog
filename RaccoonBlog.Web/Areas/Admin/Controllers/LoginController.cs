using Microsoft.AspNetCore.Mvc;
using HibernatingRhinos.Loci.Common.Models;
using RaccoonBlog.Web.Controllers;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public partial class LoginController : RaccoonController
	{
		private readonly SignInHelper _signInHelper;

		public LoginController(SignInHelper signInHelper) // ASP.NET Core: Constructor injection
		{
			_signInHelper = signInHelper;
		}

		[HttpGet]
		public virtual IActionResult Index(string returnUrl)
		{
			if (User.Identity.IsAuthenticated) // ASP.NET Core: Request.IsAuthenticated ? User.Identity.IsAuthenticated
			{
				return RedirectFromLoginPage();
			}

			return View(new LogOnModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult Index(LogOnModel input)
		{
			var user = RavenSession.GetUserByEmail(input.Login);

			if (user == null || user.ValidatePassword(input.Password) == false)
			{
				ModelState.AddModelError("UserNotExistOrPasswordNotMatch",
										 "Email and password do not match to any known user.");
			}
			else if (user.Enabled == false)
			{
				ModelState.AddModelError("NotEnabled", "The user is not enabled");
			}

			if (ModelState.IsValid)
			{
				_signInHelper.SignIn(input, true);
				return RedirectFromLoginPage(input.ReturnUrl);
			}

			return View(new LogOnModel { Login = input.Login, ReturnUrl = input.ReturnUrl });
		}

		private IActionResult RedirectFromLoginPage(string retrunUrl = null)
		{
			if (string.IsNullOrEmpty(retrunUrl))
                return RedirectToAction("Index", "Posts", new { area = "" });
            //return RedirectToRoute("homepage");
            return Redirect(retrunUrl);
		}

		[HttpGet]
		public virtual IActionResult LogOut(string returnurl)
		{
			_signInHelper.SignOut();
			return RedirectFromLoginPage(returnurl);
		}

		// ASP.NET Core: ChildActionOnly removed, use ViewComponent instead
		// [ChildActionOnly]
		public virtual IActionResult CurrentUser()
		{
			if (User.Identity.IsAuthenticated == false) // ASP.NET Core: Request.IsAuthenticated ? User.Identity.IsAuthenticated
				return View(new CurrentUserViewModel());

			var user = RavenSession.GetUserByEmail(User.Identity.Name); // ASP.NET Core: HttpContext.User ? User
			return View(new CurrentUserViewModel { FullName = user.FullName }); // TODO: we don't really need a VM here
		}
	}
}