using System;
using System.Web;
using System.Web.Hosting;
using FluentScheduler;
using HibernatingRhinos.Loci.Common.Controllers;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.Services;
using Raven.Client;

namespace RaccoonBlog.Web.Infrastructure.Jobs
{
    public class RedditIntegration : JobBase
    {
        protected override void Run(IDocumentSession session)
        {
            var submitToReddit = new SubmitToRedditStrategy(session);
            submitToReddit.SubmitPostsToReddit(DateTimeOffset.UtcNow);
        }
    }
}