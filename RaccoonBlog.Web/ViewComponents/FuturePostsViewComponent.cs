using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.ViewComponents
{
	public class FuturePostsViewComponent : ViewComponent
	{
		private readonly IDocumentSession _session;

		public FuturePostsViewComponent(IDocumentSession session)
		{
			_session = session;
		}

		public IViewComponentResult Invoke(string sectionTitle)
		{
			ViewBag.SectionTitle = sectionTitle;

			var futurePosts = _session.Query<Post>()
				.Statistics(out var stats)
				.Where(x => x.PublishAt > DateTimeOffset.Now.AsMinutes())
				.Select(x => new Post { Title = x.Title, PublishAt = x.PublishAt })
				.OrderBy(x => x.PublishAt)
				.Take(5)
				.ToList();

			var lastPost = _session.Query<Post>()
				.OrderByDescending(x => x.PublishAt)
				.Select(x => new Post { PublishAt = x.PublishAt })
				.FirstOrDefault();

			var viewModel = new FuturePostsViewModel
			{
				LastPostDate = lastPost == null ? null : (DateTimeOffset?)lastPost.PublishAt,
				TotalCount = (int)stats.TotalResults,
				Posts = futurePosts.MapTo<FuturePostViewModel>()
			};

			return View(viewModel);
		}
	}
}
