using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Helpers.Attributes;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Linq;

namespace RaccoonBlog.Web.Areas.Admin.Controllers
{
	public partial class SectionsController : AdminController
	{
        private readonly CacheSignalService _cacheSignal;
        public SectionsController(IDocumentStore documentStore, IDocumentSession ravenSession, CacheSignalService cacheSignal)
        : base(documentStore, ravenSession)
        {
            _cacheSignal = cacheSignal;
        }

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
            id = Uri.UnescapeDataString(id);

			var section = RavenSession.Load<Section>(id);
			if (section == null)
				return NotFound("Section does not exist.");

            return View(section);
		}

		[HttpPost]
		public virtual IActionResult Activate(string id, bool activate)
		{
            id = Uri.UnescapeDataString(id);

            var section = RavenSession.Load<Section>(id);
			if (section == null)
				return NotFound("Section does not exist.");

			section.IsActive = activate;

            _cacheSignal.Invalidate(CacheKeys.SectionArea);

            return StatusCode(StatusCodes.Status200OK);
		}

        [HttpPost]
        public virtual IActionResult Update(Section section)
        {
            if (!ModelState.IsValid)
                return View("Edit", section);

            if (!string.IsNullOrEmpty(section.Id))
            {
                section.Id = Uri.UnescapeDataString(section.Id);
            }

            if (section.Position == 0)
            {
                section.Position = RavenSession.Query<Section>()
                    .OrderByDescending(sec => sec.Position)
                    .Select(sec => sec.Position)
                    .FirstOrDefault() + 1;
            }

            RavenSession.Store(section);

            _cacheSignal.Invalidate(CacheKeys.SectionArea);

            return RedirectToAction("Index");
        }

        [HttpPost]
		public virtual IActionResult Delete(string id)
		{
            id = Uri.UnescapeDataString(id);

            var section = RavenSession.Load<Section>(id);
			if (section == null)
				return NotFound("Section does not exist.");

			RavenSession.Delete(section);

            _cacheSignal.Invalidate(CacheKeys.SectionArea);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return Json(new { Success = true });
			}
			return RedirectToAction("Index");
		}

		[AjaxOnly]
		[HttpPost]
		public virtual IActionResult SetPosition(string id, int newPosition)
		{
            id = Uri.UnescapeDataString(id);

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

            _cacheSignal.Invalidate(CacheKeys.SectionArea);

            return Json(new { success = true });
		}
	}
}
