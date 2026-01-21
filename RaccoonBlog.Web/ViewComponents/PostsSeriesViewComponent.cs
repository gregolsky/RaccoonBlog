using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Infrastructure.Indexes;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.ViewComponents
{
	public class PostsSeriesViewComponent : ViewComponent
	{
		private readonly IDocumentSession _session;

		public PostsSeriesViewComponent(IDocumentSession session)
		{
			_session = session;
		}

		public IViewComponentResult Invoke(string sectionTitle)
		{
			ViewBag.SectionTitle = sectionTitle;

			var series = _session.Query<Posts_Series.Result, Posts_Series>()
				.Where(x => x.Count > 1)
				.OrderByDescending(x => x.MaxDate)
				.Take(5)
				.ToList();

			var vm = series.Select(result => new RecentSeriesViewModel
			{
				SeriesId = result.SeriesId,
				SeriesSlug = SlugConverter.TitleToSlug(result.Series),
				SeriesTitle = TitleConverter.ToSeriesTitle(result.Posts.First().Title),
				PostsCount = result.Count,
				PostInformation = result.Posts
									.OrderByDescending(post => post.PublishAt)
									.FirstOrDefault(post => post.PublishAt <= DateTimeOffset.Now)
			})
			.Where(x => x.PostInformation != null)
			.ToList();

			return View(vm);
		}
	}
}
