using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using RavenDbBlog.Core.Models;
using RavenDbBlog.ViewModels;

namespace RavenDbBlog.Controllers
{
    public class LoginController : AbstractController
    {
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectFromLoginPage();
            }

            return View(new LoginInput { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public ActionResult Login(LoginInput input)
        {
            var user = Session.GetUserByEmail(input.Email);

        	if (user == null || user.ValidatePassword(input.Password) == false)
            {
                ModelState.AddModelError("UserNotExistOrPasswordNotMatch", 
					"Email and password are not match.");
            }
			else if(user.Enabled == false)
			{
				ModelState.AddModelError("NotEnabled", "The user is not enabled");
			}

            if (ModelState.IsValid)
            {
                FormsAuthentication.SetAuthCookie(input.Email, false);
                return RedirectFromLoginPage(input.ReturnUrl);
            }

        	return View(new LoginInput {Email = input.Email, ReturnUrl = input.ReturnUrl});
        }

        private ActionResult RedirectFromLoginPage(string retrunUrl = null)
        {
            if (string.IsNullOrEmpty(retrunUrl))
                return RedirectToRoute("Default");
            return Redirect(retrunUrl);
        }

       

        [HttpGet]
        public ActionResult LogOut(string returnurl)
        {
            FormsAuthentication.SignOut();
            return RedirectFromLoginPage(returnurl);
        }

        [ChildActionOnly]
        public ActionResult CurrentUser()
        {
			if (Request.IsAuthenticated == false)
				return View(new CurrentUserViewModel());

        	var user = Session.GetUserByEmail(HttpContext.User.Identity.Name);
        	return View(new CurrentUserViewModel {FullName = user.FullName});
        }

        [ChildActionOnly]
        public ActionResult AdministrationPanel()
        {
            if (Request.IsAuthenticated == false)
                return View(new CurrentUserViewModel());

            var user = Session.GetUserByEmail(HttpContext.User.Identity.Name);
            return View(new CurrentUserViewModel { FullName = user.FullName });
        }
    }
}