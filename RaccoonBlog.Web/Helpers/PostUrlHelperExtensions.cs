using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace RaccoonBlog.Web.Helpers
{
    public static class PostUrlHelperExtensions
    {
        /// <summary>
        /// Generates a URL for a post series
        /// </summary>
        public static string Series(this IUrlHelper helper, string seriesId, string seriesSlug)
        {
            return helper.Action(
                action: "Series",
                controller: "Posts",
                values: new { seriesId, seriesSlug });
        }

        /// <summary>
        /// Generates a URL for a post details page
        /// </summary>
        public static string Post(this IUrlHelper helper, string id, string slug)
        {
            return helper.Action(
                action: "Details",
                controller: "PostDetails",
                values: new { id, slug });
        }
    }
}