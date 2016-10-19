using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using Raven.Client;
using Raven.Client.Linq;

namespace RaccoonBlog.Web.Infrastructure
{
    public static class PostQueries
    {
        public static IRavenQueryable<Post> QueryPostsDefault(this IDocumentSession documentSession)
        {
            return documentSession.Query<Post>()
                .Where(x => x.IsDeleted == false);
        }

        public static IRavenQueryable<Post> QueryPublicPosts(this IDocumentSession documentSession)
        {
            return documentSession.QueryPostsDefault()
				.Where(post => post.PublishAt < DateTimeOffset.Now.AsMinutes());
        }
    }
}