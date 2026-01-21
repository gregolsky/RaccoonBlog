using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Indexes;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.ViewComponents
{
	public class PostsStatisticsViewComponent : ViewComponent
	{
		private readonly IDocumentSession _session;

		public PostsStatisticsViewComponent(IDocumentSession session)
		{
			_session = session;
		}

		public IViewComponentResult Invoke()
		{
			var statistics = _session.Query<Posts_Statistics.ReduceResult, Posts_Statistics>()
				.FirstOrDefault() ?? new Posts_Statistics.ReduceResult();

			var viewModel = statistics.MapTo<PostsStatisticsViewModel>();

			return View(viewModel);
		}
	}
}
