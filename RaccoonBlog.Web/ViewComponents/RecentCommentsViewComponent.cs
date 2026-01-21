using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.ViewComponents
{
	public class RecentCommentsViewComponent : ViewComponent
	{
		private readonly IDocumentSession _session;

		public RecentCommentsViewComponent(IDocumentSession session)
		{
			_session = session;
		}

		public IViewComponentResult Invoke(string sectionTitle)
		{
			ViewBag.SectionTitle = sectionTitle;

			var commentsTuples = _session.QueryForRecentComments(q => q.Take(5));

			var result = new List<RecentCommentViewModel>();
			foreach (var commentsTuple in commentsTuples)
			{
				var recentCommentViewModel = commentsTuple.Item1.MapTo<RecentCommentViewModel>();
				commentsTuple.Item2.MapPropertiesToInstance(recentCommentViewModel);
				result.Add(recentCommentViewModel);
			}

			return View(result);
		}
	}
}
