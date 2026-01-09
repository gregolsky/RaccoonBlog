using System.Linq;
using Microsoft.AspNetCore.Mvc;
using HibernatingRhinos.Loci.Common.Models;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.Areas.Admin.Controllers
{
	public partial class UsersController : AdminController
	{
		public virtual IActionResult Index()
		{
			var users = RavenSession.Query<User>()
				.OrderBy(u => u.FullName)
				.ToList();

			var vm = users.MapTo<UserSummeryViewModel>();
			return View("List", vm);
		}

		[HttpGet]
		public virtual IActionResult Add()
		{
			return View("Edit", new UserInput());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult Add(UserInput input)
		{
			if (!ModelState.IsValid)
				return View("Edit", input);

			var user = new User();
			input.MapPropertiesToInstance(user);
			RavenSession.Store(user);
			return RedirectToAction("Index");
		}

		[HttpGet]
		public virtual IActionResult Edit(string id)
		{
			var user = RavenSession.Load<User>("users/" + id);
			if (user == null)
				return NotFound("User does not exist.");
			return View(user.MapTo<UserInput>());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult Update(UserInput input)
		{
			if (!ModelState.IsValid)
				return View("Edit", input);

			var user = RavenSession.Load<User>("users/" + input.Id) ?? new User();
			input.MapPropertiesToInstance(user);
			RavenSession.Store(user);
			return RedirectToAction("Index");
		}

		[HttpGet]
		public virtual IActionResult ChangePassword(string id)
		{
			var user = RavenSession.Load<User>("users/" + id);
			if (user == null)
				return NotFound("User does not exist.");

			return View(new ChangePasswordModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult ChangePassword(ChangePasswordModel input)
		{
			if (!ModelState.IsValid)
				return View("ChangePassword", input);

			var user = RavenSession.Load<User>("users/" + input.Id);
			if (user == null)
				return NotFound("User does not exist.");

			if (user.ValidatePassword(input.OldPassword) == false)
			{
				ModelState.AddModelError("OldPassword", "Old password did not match existing password");
			}

			if (ModelState.IsValid == false)
				return View(input);

			user.SetPassword(input.NewPassword);
			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult SetActivation(string id, bool isActive)
		{
			var user = RavenSession.Load<User>("users/" + id);
			if (user == null)
				return NotFound("User does not exist.");

			user.Enabled = isActive;

			return RedirectToAction("Index");
		}
	}
}
