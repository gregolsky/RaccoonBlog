using System.Threading.Tasks;
using Xunit;

namespace RaccoonBlog.IntegrationTests.Routing
{
    /// <summary>
    /// Tests for public area routes.
    /// Modernized for ASP.NET Core/.NET 8 - replaces MvcContrib test helpers.
    /// These tests verify that routes are properly configured and accessible.
    /// </summary>
    public class RoutesTests : RoutingTestBase
    {
        [Fact]
        public async Task DefaultRoute()
        {
            await AssertRouteExists("/", "GET");
        }

        [Fact]
        public async Task SyndicationControllerRoutes_Rss()
        {
            await AssertRouteExists("/rss", "GET");
        }

        [Fact]
        public async Task SyndicationControllerRoutes_RssWithTag()
        {
            await AssertRouteExists("/rss/tag-name", "GET");
        }

        [Fact]
        public async Task SyndicationControllerRoutes_Rsd()
        {
            await AssertRouteExists("/rsd", "GET");
        }

        [Fact]
        public async Task SyndicationControllerRoutes_LegacyRss()
        {
            await AssertRouteExists("/rss.aspx", "GET");
        }

        [Fact]
        public async Task PostsController_Index()
        {
            await AssertRouteExists("/", "GET");
        }

        [Fact]
        public async Task PostsController_Tag()
        {
            await AssertRouteExists("/tags/tag-name", "GET");
        }

        [Fact]
        public async Task PostsController_Archive_Year()
        {
            await AssertRouteExists("/archive/2011", "GET");
        }

        [Fact]
        public async Task PostsController_Archive_YearMonth()
        {
            await AssertRouteExists("/archive/2011/4", "GET");
        }

        [Fact]
        public async Task PostsController_Archive_YearMonthDay()
        {
            await AssertRouteExists("/archive/2011/4/24", "GET");
        }

        [Fact]
        public async Task LegacyPostController_RedirectLegacyPost()
        {
            await AssertRouteExists("/archive/2011/4/24/legacy-post-title.aspx", "GET");
        }

        [Fact]
        public async Task LegacyPostController_RedirectLegacyArchive()
        {
            await AssertRouteExists("/archive/2011/4/24.aspx", "GET");
        }

        [Fact]
        public async Task PostDetailsController_DetailsById()
        {
            await AssertRouteExists("/1024", "GET");
        }

        [Fact]
        public async Task PostDetailsController_DetailsByIdAndSlug()
        {
            await AssertRouteExists("/1024/blog-post-title", "GET");
        }

        [Fact]
        public async Task PostDetailsController_Comment_Get()
        {
            await AssertRouteExists("/1024/comment", "GET");
        }

        [Fact]
        public async Task PostDetailsController_Comment_Post()
        {
            await AssertRouteExists("/1024/comment", "POST");
        }

        [Fact]
        public async Task SectionController_List()
        {
            await AssertRouteExists("/section/list", "GET");
        }

        [Fact]
        public async Task SectionController_TagsList()
        {
            await AssertRouteExists("/section/tagslist", "GET");
        }

        [Fact]
        public async Task SectionController_FuturePosts()
        {
            await AssertRouteExists("/section/futureposts", "GET");
        }

        [Fact]
        public async Task SectionController_ArchivesList()
        {
            await AssertRouteExists("/section/archiveslist", "GET");
        }

        [Fact]
        public async Task SectionController_PostsStatistics()
        {
            await AssertRouteExists("/section/postsstatistics", "GET");
        }

        [Fact]
        public async Task SearchController_SearchResult()
        {
            await AssertRouteExists("/search", "GET");
        }

        [Fact]
        public async Task CssController_Merge()
        {
            await AssertRouteExists("/css", "GET");
        }
    }
}
