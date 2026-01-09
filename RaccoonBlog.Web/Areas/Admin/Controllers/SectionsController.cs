using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaccoonBlog.Web.Helpers.Attributes;
using RaccoonBlog.Web.Models;

namespace RaccoonBlog.Web.Areas.Admin.Controllers
{
	public partial class SectionsController : AdminController
	{
		public virtual IActionResult Index()
		{
			var sections = RavenSession.Query<Section>()
				.OrderBy(x => x.Position)
				.ToList();

			return View("List", sections);
		}

		[HttpGet]
		public virtual IActionResult Add()
		{
			return View("Edit", new Section());
		}

		[HttpGet]
		public virtual IActionResult Edit(string id)
		{
			var section = RavenSession.Load<Section>(id);
			if (section == null)
				return NotFound("Section does not exist.");

			return View(section);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult Activate(string id, bool activate)
		{
			var section = RavenSession.Load<Section>(id);
			if (section == null)
				return NotFound("Section does not exist.");

			section.IsActive = activate;

			// ASP.NET Core: Cache invalidation moved to separate service
			// OutputCacheManager.RemoveItems("Section");

			return StatusCode(StatusCodes.Status200OK);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult Update(Section section)
		{
			if (!ModelState.IsValid)
				return View("Edit", section);

			if (section.Position == 0)
			{
				section.Position = RavenSession.Query<Section>()
					.OrderByDescending(sec => sec.Position)
					.Select(sec => sec.Position)
					.FirstOrDefault() + 1;
			}
			RavenSession.Store(section);

			// ASP.NET Core: Cache invalidation moved to separate service
			// OutputCacheManager.RemoveItems("Section");

			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult Delete(string id)
		{
			var section = RavenSession.Load<Section>(id);
			if (section == null)
				return NotFound("Section does not exist.");

			RavenSession.Delete(section);

			// ASP.NET Core: Cache invalidation moved to separate service
			// OutputCacheManager.RemoveItems("Section");

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") // ASP.NET Core: IsAjaxRequest() replacement
			{
				return Json(new { Success = true });
			}
			return RedirectToAction("Index");
		}

		[AjaxOnly]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult SetPosition(string id, int newPosition)
		{
			var section = RavenSession.Load<Section>(id);
			if (section == null)
				return Json(new {success = false, message = string.Format("There is no post with id {0}", id)});

			if (section.Position == newPosition)
				return Json(new {success = false, message = string.Format("The {0} section has already this position", section.Title)});

			if (section.Position > newPosition)
			{
				var sections = RavenSession.Query<Section>()
					.Where(s => s.Position >= newPosition && s.Position < section.Position)
					.OrderBy(s => s.Position)
					.ToList();

				foreach (var s in sections)
				{
					s.Position++;
				}
			}
			else if (section.Position < newPosition)
			{
				var sections = RavenSession.Query<Section>()
					.Where(s => s.Position < newPosition && s.Position >= section.Position)
					.OrderBy(s => s.Position)
					.ToList();

				foreach (var s in sections)
				{
					s.Position--;
				}
			}

			section.Position = newPosition;

			// ASP.NET Core: Cache invalidation moved to separate service
			// OutputCacheManager.RemoveItems("Section");

			return Json(new { success = true });
		}
	}
}
