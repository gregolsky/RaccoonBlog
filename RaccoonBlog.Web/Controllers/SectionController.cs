using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Indexes;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;
using RaccoonBlog.Web.Infrastructure.Common;

namespace RaccoonBlog.Web.Controllers
{
	using DevTrends.MvcDonutCaching;

	public partial class SectionController : AggresivelyCachingRacconController
    {
	    protected override TimeSpan CacheDuration => TimeSpan.FromMinutes(6);

        [ChildActionOnly]
		public virtual ActionResult PostsSeries(string sectionTitle)
        {
            ViewBag.SectionTitle = sectionTitle;

            var series = RavenSession.Query<Posts_Series.Result, Posts_Series>()
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

		[ChildActionOnly]
		public virtual ActionResult FuturePosts(string sectionTitle)
		{
            ViewBag.SectionTitle = sectionTitle;

			var futurePosts = RavenSession.Query<Post>()
				.Statistics(out var stats)
				.Where(x => x.PublishAt > DateTimeOffset.Now.AsMinutes())
				.Select(x => new Post {Title = x.Title, PublishAt = x.PublishAt})
				.OrderBy(x => x.PublishAt)
				.Take(5)
				.ToList();

			var lastPost = RavenSession.Query<Post>()
				.OrderByDescending(x => x.PublishAt)
				.Select(x => new Post { PublishAt = x.PublishAt })
				.FirstOrDefault();
				

			return View(
				new FuturePostsViewModel
				{
					LastPostDate = lastPost == null ? null : (DateTimeOffset?)lastPost.PublishAt,
					TotalCount = (int)stats.TotalResults,
					Posts = futurePosts.MapTo<FuturePostViewModel>()
				});
		}

		[ChildActionOnly]
		[DonutOutputCache(Duration = 300)]
		public virtual ActionResult List()
		{
			if (true.Equals(HttpContext.Items["CurrentlyProcessingException"]))
				return View(new SectionDetails[0]);

			var sections = Sections
                .Where(s => s.IsActive && s.IsRightSide)
				.OrderBy(x => x.Position)
				.ToList();

			return View(sections.MapTo<SectionDetails>());
		}

        [ChildActionOnly]
		[DonutOutputCache(Duration = 3600)]
		public virtual ActionResult ContactMe()
        {
            return View();
        }
        
		[ChildActionOnly]
		[OutputCache(Duration = 3600)]
		public virtual ActionResult TagsList()
		{
			var mostRecentTag = new DateTimeOffset(DateTimeOffset.Now.Year - 2,
												   DateTimeOffset.Now.Month,
												   1, 0, 0, 0,
												   DateTimeOffset.Now.Offset);

			var tags = RavenSession.Query<Tags_Count.ReduceResult, Tags_Count>()
				.Where(x => x.Count > BlogConfig.MinNumberOfPostForSignificantTag && x.LastSeenAt > mostRecentTag)
				.OrderBy(x => x.Name)
				.ToList();

			return View(tags.MapTo<TagsListViewModel>());
		}

		[ChildActionOnly]
		[OutputCache(Duration = 3600)]
		public virtual ActionResult ArchivesList()
		{
			var now = DateTime.Now;

            var dates = RavenSession.Query<Posts_ByMonthPublished_Count.ReduceResult, Posts_ByMonthPublished_Count>()
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(1024)
                .Where(x => x.Year < now.Year || x.Year == now.Year && x.Month <= now.Month)
                .ToList();
            
			return View(dates);
		}

		[ChildActionOnly]
		[OutputCache(Duration = 360)]
		public virtual ActionResult PostsStatistics()
		{
			var statistics = RavenSession.Query<Posts_Statistics.ReduceResult, Posts_Statistics>()
				.FirstOrDefault() ?? new Posts_Statistics.ReduceResult();

			return View(statistics.MapTo<PostsStatisticsViewModel>());
		}

		[ChildActionOnly]
		public virtual ActionResult RecentComments(string sectionTitle)
		{
		    ViewBag.SectionTitle = sectionTitle;
			var commentsTuples = RavenSession.QueryForRecentComments(q => q.Take(5));

			var result = new List<RecentCommentViewModel>();
			foreach (var commentsTuple in commentsTuples)
			{
				var recentCommentViewModel = commentsTuple.Item1.MapTo<RecentCommentViewModel>();
				commentsTuple.Item2.MapPropertiesToInstance(recentCommentViewModel);
				result.Add(recentCommentViewModel);
			}
			return View(result);
		}

		[ChildActionOnly]
		public virtual ActionResult AdministrationPanel()
		{
			var user = RavenSession.GetCurrentUser();

			var vm = new CurrentUserViewModel();
			if (user != null)
			{
				vm.FullName = user.FullName;
			}
			return View(vm);
		}
    }
}
