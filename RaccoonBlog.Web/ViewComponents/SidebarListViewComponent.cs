using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.ViewComponents
{
	public class SidebarListViewComponent : ViewComponent
	{
		private readonly IDocumentSession _session;

		public SidebarListViewComponent(IDocumentSession session)
		{
			_session = session;
		}

		public IViewComponentResult Invoke()
		{
            // Check if we're currently processing an exception
            if (true.Equals(HttpContext.Items["CurrentlyProcessingException"]))
			{
				return View(new SectionDetails[0]);
			}

			var sections = _session.Query<Section>()
				.Where(s => s.IsActive && s.IsRightSide)
				.OrderBy(x => x.Position)
				.ToList();

			var viewModel = sections.MapTo<SectionDetails>();

			return View(viewModel);
		}
	}
}
