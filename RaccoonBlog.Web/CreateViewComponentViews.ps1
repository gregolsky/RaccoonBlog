# PowerShell script to create View Component view files

$basePath = "C:\Work\RaccoonBlog\RaccoonBlog.Web\Views\Shared\Components"

# Create directories
$components = @("TagsList", "ArchivesList", "SidebarList", "PullDown", "PostsSeries", "FuturePosts", "PostsStatistics", "RecentComments")
foreach ($component in $components) {
    $componentPath = Join-Path $basePath $component
    if (!(Test-Path $componentPath)) {
        New-Item -ItemType Directory -Path $componentPath -Force | Out-Null
        Write-Host "Created directory: $component"
    }
}

# TagsList Default.cshtml
$tagsListView = @'
@model IList<RaccoonBlog.Web.ViewModels.TagsListViewModel>
<nav id="tags">
	<ul>
		@{
			var url = Url.Action("Tag", "Posts", new { Slug = "_s_"}); // Url.Action is slow
			foreach (var tag in Model)
			{
				<li>
					<a href="@url.Replace("_s_", tag.Slug)">@tag.Name</a>&nbsp;<span>(@tag.Count)</span>&nbsp;<a href="@Url.Action("Rss", "Syndication", new { Tag = tag.Slug })" class="rss">rss</a>
				</li>
			}
		}
	</ul>
</nav>
'@
Set-Content -Path (Join-Path $basePath "TagsList\Default.cshtml") -Value $tagsListView
Write-Host "Created TagsList/Default.cshtml"

# ArchivesList Default.cshtml
$archivesListView = @'
@model IList<RaccoonBlog.Web.Infrastructure.Indexes.Posts_ByMonthPublished_Count.ReduceResult>
<nav id="archive">
	<ul>
		@{
			var url = Url.Action("Archive", "Posts", new { Year = 9999, Month = 99 }); // Url.Action is slow

			foreach (var g in Model.GroupBy(x => x.Year))
			{
				<li>
					<a href="#">@g.Key</a>
					<ul>
						@foreach (var postCount in g)
						{
							<li><a href="@(url.Replace("9999", postCount.Year.ToString()).Replace("99", postCount.Month.ToString()))"> @(new DateTime(postCount.Year, @postCount.Month, 1).ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture)) </a> (@postCount.Count)</li>
						}
					</ul>
				</li>
			}
		}
	</ul>
</nav>
'@
Set-Content -Path (Join-Path $basePath "ArchivesList\Default.cshtml") -Value $archivesListView
Write-Host "Created ArchivesList/Default.cshtml"

# SidebarList Default.cshtml
$sidebarListView = @'
@model IList<RaccoonBlog.Web.ViewModels.SectionDetails>

@foreach (var item in Model)
{
	if (item.IsActionSection())
	{
		@if (item.ControllerName == "Section" && item.ActionName == "PostsSeries")
		{
			@Component.Invoke("PostsSeries", new { sectionTitle = item.Title })
		}
		else if (item.ControllerName == "Section" && item.ActionName == "FuturePosts")
		{
			@Component.Invoke("FuturePosts", new { sectionTitle = item.Title })
		}
		else if (item.ControllerName == "Section" && item.ActionName == "PostsStatistics")
		{
			@Component.Invoke("PostsStatistics")
		}
		else if (item.ControllerName == "Section" && item.ActionName == "RecentComments")
		{
			@Component.Invoke("RecentComments", new { sectionTitle = item.Title })
		}
		else
		{
			<div class="placeholder">
				<p>Item: @item.ControllerName/@item.ActionName</p>
			</div>
		}
	}
	else
	{
		<div id="@Html.ConvertSectionTitleToId(item.Title)">
			<h4>@(item.Title)</h4>
			@(item.Body)
		</div>
	}
}
'@
Set-Content -Path (Join-Path $basePath "SidebarList\Default.cshtml") -Value $sidebarListView
Write-Host "Created SidebarList/Default.cshtml"

# PullDown Default.cshtml
$pullDownView = @'
@model IList<RaccoonBlog.Web.ViewModels.SectionDetails>

@foreach (var item in Model)
{
	if (item.IsActionSection())
	{
		@if (item.ControllerName == "Section" && item.ActionName == "PostsSeries")
		{
			@Component.Invoke("PostsSeries", new { sectionTitle = item.Title })
		}
		else if (item.ControllerName == "Section" && item.ActionName == "FuturePosts")
		{
			@Component.Invoke("FuturePosts", new { sectionTitle = item.Title })
		}
		else if (item.ControllerName == "Section" && item.ActionName == "PostsStatistics")
		{
			@Component.Invoke("PostsStatistics")
		}
		else if (item.ControllerName == "Section" && item.ActionName == "RecentComments")
		{
			@Component.Invoke("RecentComments", new { sectionTitle = item.Title })
		}
		else
		{
			<div class="placeholder">
				<p>Item: @item.ControllerName/@item.ActionName</p>
			</div>
		}
	}
	else
	{
		<div id="@Html.ConvertSectionTitleToId(item.Title)" class="pull-down-item">
			<h4>@(item.Title)</h4>
			@(item.Body)
		</div>
	}
}
'@
Set-Content -Path (Join-Path $basePath "PullDown\Default.cshtml") -Value $pullDownView
Write-Host "Created PullDown/Default.cshtml"

# PostsSeries Default.cshtml
$postsSeriesView = @'
@using RaccoonBlog.Web.Infrastructure.Common
@model List<RaccoonBlog.Web.ViewModels.RecentSeriesViewModel>

<div id="postsSeries">
    <h4>@ViewBag.SectionTitle.ToUpper()</h4>

    @if (Model.Count > 0)
    {
        <ol>
            @foreach (var series in Model)
            {
                <li>
                    <a href="@Url.Action("Series", "Posts", new {series.SeriesId, series.SeriesSlug})">
                        <strong>
                            @Html.Raw(series.SeriesTitle)
                        </strong>
                    </a> <em>(@series.PostsCount)</em>:<br/>
                    <em>@series.PostInformation.PublishAt.ToString("dd MMM yyyy")</em> - @Html.Raw(TitleConverter.ToPostTitle(@series.PostInformation.Title))
                </li>
            }
        </ol>

        <a href="@Url.Action("PostsSeries", "Series")">View all series</a>
    }
</div>
'@
Set-Content -Path (Join-Path $basePath "PostsSeries\Default.cshtml") -Value $postsSeriesView
Write-Host "Created PostsSeries/Default.cshtml"

# FuturePosts Default.cshtml
$futurePostsView = @'
@using System.Globalization
@model RaccoonBlog.Web.ViewModels.FuturePostsViewModel

<div id="futurePosts">
    <h4>@ViewBag.SectionTitle.ToUpper()</h4>
    @if (Model.Posts.Count > 0)
    {
        <ol>
            @foreach (var futurePost in Model.Posts)
            {
                <li>@futurePost.Title - <em>@futurePost.Time</em></li>
            }
        </ol>

        if (Model.TotalCount > Model.Posts.Count)
        {
            <p>And @(Model.TotalCount - Model.Posts.Count) more posts are pending...</p>
        }
        if (Model.LastPostDate != null)
        {
            <p>There are posts all the way to @Model.LastPostDate.Value.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture)</p>
        }
    }
    else
    {
        <p>No future posts left, oh my!</p>
    }
</div>
'@
Set-Content -Path (Join-Path $basePath "FuturePosts\Default.cshtml") -Value $futurePostsView
Write-Host "Created FuturePosts/Default.cshtml"

# PostsStatistics Default.cshtml
$postsStatsView = @'
@model RaccoonBlog.Web.ViewModels.PostsStatisticsViewModel

@if (Model != null)
{
<div class="hstack flex-wrap justify-content-center margin-bottom">
	<div class="vstack">
		Posts: <strong>@Model.PostsCount.ToString("0,0")</strong>
	</div>
	<span class="separator">|</span>
	<div class="vstack">
		Comments: <strong>@Model.CommentsCount.ToString("0,0")</strong>
	</div>
</div>
}
'@
Set-Content -Path (Join-Path $basePath "PostsStatistics\Default.cshtml") -Value $postsStatsView
Write-Host "Created PostsStatistics/Default.cshtml"

# RecentComments Default.cshtml
$recentCommentsView = @'
@model IList<RaccoonBlog.Web.ViewModels.RecentCommentViewModel>

<div id="recentComments">
    <h4>@ViewBag.SectionTitle.ToUpper()</h4>
    <ul>
        @foreach (var comment in Model)
        {
            var postUrl = Url.Action("Details", "PostDetails", new { Id = comment.PostId, Slug = comment.PostSlug });

            <li>
                <div>
                    <a href="@(postUrl)#comment@(comment.CommentId)">
                        <strong class="comment-body">@comment.ShortBody</strong>
                    </a>
                </div>
	            <div>
		            <strong>By</strong>&nbsp;@comment.Author on <em>@Html.Raw(@comment.PostTitle)</em>
	            </div>
            </li>
        }
    </ul>
</div>
'@
Set-Content -Path (Join-Path $basePath "RecentComments\Default.cshtml") -Value $recentCommentsView
Write-Host "Created RecentComments/Default.cshtml"

Write-Host "`nAll View Component view files created successfully!"
Write-Host "Location: $basePath"
