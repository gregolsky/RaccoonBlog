using System;
using FluentScheduler;

namespace RaccoonBlog.Web.Infrastructure.Jobs
{
    public class SocialNetworkIntegrationJobsRegistry : Registry
    {
        public SocialNetworkIntegrationJobsRegistry()
        {
            // TODO: Reddit integration temporarily disabled due to RedditSharp API breaking changes
            // Uncomment when Reddit integration is updated to RedditSharp 2.0+ API
            
            /*
            Schedule<RedditIntegration>()
                .WithName("RedditIntegration")
                .NonReentrant()
                .ToRunEvery(5)
                .Minutes();
            */
        }
    }
}