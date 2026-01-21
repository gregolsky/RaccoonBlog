using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Indexes;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.ViewComponents
{
	public class TagsListViewComponent : ViewComponent
	{
		private readonly IDocumentSession _session;
		private readonly BlogConfig _blogConfig;

		public TagsListViewComponent(IDocumentSession session, BlogConfig blogConfig)
		{
			_session = session;
			_blogConfig = blogConfig;
		}

		public IViewComponentResult Invoke()
		{
			var mostRecentTag = new DateTimeOffset(
				DateTimeOffset.Now.Year - 2,
				DateTimeOffset.Now.Month,
				1, 0, 0, 0,
				DateTimeOffset.Now.Offset);

			var tags = _session.Query<Tags_Count.ReduceResult, Tags_Count>()
				.Where(x => x.Count > _blogConfig.MinNumberOfPostForSignificantTag && x.LastSeenAt > mostRecentTag)
				.OrderBy(x => x.Name)
				.ToList();

			var viewModel = tags.MapTo<TagsListViewModel>();

			return View(viewModel);
		}
	}
}
