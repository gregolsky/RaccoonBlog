using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RaccoonBlog.Web.Infrastructure.Indexes;

namespace RaccoonBlog.Web.ViewComponents
{
	public class ArchivesListViewComponent : ViewComponent
	{
		private readonly IDocumentSession _session;

		public ArchivesListViewComponent(IDocumentSession session)
		{
			_session = session;
		}

		public IViewComponentResult Invoke()
		{
			var now = DateTime.Now;

			var dates = _session.Query<Posts_ByMonthPublished_Count.ReduceResult, Posts_ByMonthPublished_Count>()
				.OrderByDescending(x => x.Year)
				.ThenByDescending(x => x.Month)
				.Take(1024)
				.Where(x => x.Year < now.Year || x.Year == now.Year && x.Month <= now.Month)
				.ToList();

			return View(dates);
		}
	}
}
