# PowerShell script to create View Component files

$basePath = "C:\Work\RaccoonBlog\RaccoonBlog.Web"
$viewComponentsPath = Join-Path $basePath "ViewComponents"

# Create ViewComponents directory if it doesn't exist
if (!(Test-Path $viewComponentsPath)) {
    New-Item -ItemType Directory -Path $viewComponentsPath -Force | Out-Null
    Write-Host "Created ViewComponents directory"
}

# TagsListViewComponent
$tagsListContent = @'
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
'@
Set-Content -Path (Join-Path $viewComponentsPath "TagsListViewComponent.cs") -Value $tagsListContent
Write-Host "Created TagsListViewComponent.cs"

# ArchivesListViewComponent
$archivesListContent = @'
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
'@
Set-Content -Path (Join-Path $viewComponentsPath "ArchivesListViewComponent.cs") -Value $archivesListContent
Write-Host "Created ArchivesListViewComponent.cs"

# SidebarListViewComponent
$sidebarListContent = @'
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
'@
Set-Content -Path (Join-Path $viewComponentsPath "SidebarListViewComponent.cs") -Value $sidebarListContent
Write-Host "Created SidebarListViewComponent.cs"

# PullDownViewComponent
$pullDownContent = @'
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.ViewComponents
{
	public class PullDownViewComponent : ViewComponent
	{
		private readonly IDocumentSession _session;

		public PullDownViewComponent(IDocumentSession session)
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

			// Get sections that are active but not on the right side (pull-down sections)
			var sections = _session.Query<Section>()
				.Where(s => s.IsActive && !s.IsRightSide)
				.OrderBy(x => x.Position)
				.ToList();

			var viewModel = sections.MapTo<SectionDetails>();

			return View(viewModel);
		}
	}
}
'@
Set-Content -Path (Join-Path $viewComponentsPath "PullDownViewComponent.cs") -Value $pullDownContent
Write-Host "Created PullDownViewComponent.cs"

# PostsSeriesViewComponent
$postsSeriesContent = @'
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
'@
Set-Content -Path (Join-Path $viewComponentsPath "PostsSeriesViewComponent.cs") -Value $postsSeriesContent
Write-Host "Created PostsSeriesViewComponent.cs"

# FuturePostsViewComponent
$futurePostsContent = @'
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
'@
Set-Content -Path (Join-Path $viewComponentsPath "FuturePostsViewComponent.cs") -Value $futurePostsContent
Write-Host "Created FuturePostsViewComponent.cs"

# PostsStatisticsViewComponent
$postsStatsContent = @'
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
'@
Set-Content -Path (Join-Path $viewComponentsPath "PostsStatisticsViewComponent.cs") -Value $postsStatsContent
Write-Host "Created PostsStatisticsViewComponent.cs"

# RecentCommentsViewComponent
$recentCommentsContent = @'
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
'@
Set-Content -Path (Join-Path $viewComponentsPath "RecentCommentsViewComponent.cs") -Value $recentCommentsContent
Write-Host "Created RecentCommentsViewComponent.cs"

Write-Host "`nAll View Component files created successfully!"
Write-Host "Location: $viewComponentsPath"
