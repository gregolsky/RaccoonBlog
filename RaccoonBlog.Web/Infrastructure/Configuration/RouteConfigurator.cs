using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace RaccoonBlog.Web.Infrastructure.Configuration;

public static class RouteConfigurator
{
    public static IEndpointRouteBuilder MapRaccoonBlogRoutes(this IEndpointRouteBuilder endpoints)
        {
            const string ravenIdRegex = @"^\d{{1,10}}(-[A-Za-z]{{1,2}})?$";
            
            endpoints.MapControllerRoute("CommentsRssFeed", "rss/comments", new { controller = "Syndication", action = "CommentsRss" });
            endpoints.MapControllerRoute("RsdFeed", "rsd", new { controller = "Syndication", action = "Rsd" });
            endpoints.MapControllerRoute("RssFeed-LegacyUrl", "rss.aspx", new { controller = "Syndication", action = "LegacyRss" });
            endpoints.MapControllerRoute("RssFeed", "rss/{tag?}", new { controller = "Syndication", action = "Rss" });
            
            endpoints.MapControllerRoute("Api-PostsByTags", "api/posts-by-tags", new { controller = "PostsApi", action = "GetPostsByTags" });
            endpoints.MapControllerRoute("SearchController-GoogleCse", "search/google_cse.xml", new { controller = "Search", action = "GoogleCse" });
            endpoints.MapControllerRoute("SearchController", "search/{action=SearchResult}", new { controller = "Search" });
            
            endpoints.MapControllerRoute("Terms", "terms", new { controller = "Legal", action = "Terms" });
            endpoints.MapControllerRoute("PrivacyPolicy", "privacy-policy", new { controller = "Legal", action = "PrivacyPolicy" });
            endpoints.MapControllerRoute("CssController", "css", new { controller = "Css", action = "Merge" });
            endpoints.MapControllerRoute("Error404", "error/404", new { controller = "Error", action = "Error404" });
            endpoints.MapControllerRoute("Error", "error", new { controller = "Error", action = "Error" });
            
            endpoints.MapControllerRoute("SeriesController-PostsSeries", "posts/series", new { controller = "Series", action = "PostsSeries" });
            endpoints.MapControllerRoute("PostsController-Series", "posts/series/{seriesId}/{seriesSlug}", new { controller = "Posts", action = "Series" });
            
            endpoints.MapControllerRoute("RedirectLegacyPostUrl", "archive/{year:int}/{month:int}/{day:int}/{slug}.aspx", new { controller = "LegacyPost", action = "RedirectLegacyPost" });
            endpoints.MapControllerRoute("RedirectLegacyArchive", "archive/{year:int}/{month:int}/{day:int}.aspx", new { controller = "LegacyPost", action = "RedirectLegacyArchive" });
            
            endpoints.MapControllerRoute("PostsByTag", "tags/{slug}", new { controller = "Posts", action = "Tag" });
            endpoints.MapControllerRoute("PostsByYearMonthDay", "archive/{year:int}/{month:int}/{day:int}", new { controller = "Posts", action = "Archive" });
            endpoints.MapControllerRoute("PostsByYearMonth", "archive/{year:int}/{month:int}", new { controller = "Posts", action = "Archive" });
            endpoints.MapControllerRoute("PostsByYear", "archive/{year:int}", new { controller = "Posts", action = "Archive" });
            
            endpoints.MapControllerRoute(
                name: "PostDetailsController-Comment",
                pattern: "{id:regex(" + ravenIdRegex + ")}/comment",
                defaults: new { controller = "PostDetails", action = "Comment" });

            endpoints.MapControllerRoute(
                name: "PostDetailsController-Details",
                pattern: "{id:regex(" + ravenIdRegex + ")}/{slug?}",
                defaults: new { controller = "PostDetails", action = "Details" });

            return endpoints;
        }
}