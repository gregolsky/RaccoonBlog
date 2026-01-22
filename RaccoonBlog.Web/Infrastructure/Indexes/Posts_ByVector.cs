using System;
using System.Linq;
using RaccoonBlog.Web.Models;
using Raven.Client.Documents.Indexes;
using System.Collections.Generic;

namespace RaccoonBlog.Web.Infrastructure.Indexes
{
    // DISABLED: Vector Search requires RavenDB v7+ 
    // This index uses LoadVector() which is not available in v6.0.104
    // Temporarily disabled for .NET 8 migration compatibility
    
    /*
    public class Posts_ByVector : AbstractIndexCreationTask<Post, Posts_ByTag.Query>
    {
        public class Query
        {
            public string Id { get; set; }

            public string Title { get; set; }

            public DateTimeOffset PublishAt { get; set; }

            public object Vector { get; set; }
            
            public ICollection<string> Tags { get; set; }
        }

        public Posts_ByVector()
        {
            SearchEngineType = Raven.Client.Documents.Indexes.SearchEngineType.Corax;
            Map = posts => from post in posts
                           where post.PublishAt != null
                           select new
                           {
                               Vector = LoadVector("Body", "postsbyvector"),
                               PublishAt = post.PublishAt,
                               Tags = post.Tags
                           };
        }
    }
    */
}