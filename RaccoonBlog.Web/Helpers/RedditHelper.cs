﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using RaccoonBlog.Web.Infrastructure;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.Models.SocialNetwork;
using RaccoonBlog.Web.Services.Reddit;
using Raven.Client;
using Raven.Client.Linq;

namespace RaccoonBlog.Web.Helpers
{
    public class RedditHelper
    {
        public const string SendToRedditTag = "reddit";

        public static string SubmitUrl(string subredditName, Post post)
        {
            var queryStringData = new NameValueCollection()
            {
                { "url", PostHelper.Url(post) },
                { "title", post.Title },
                { "resubmit", false.ToString() }
            };
            var queryString = ToQueryString(queryStringData);
            return $"https://www.reddit.com/r/{subredditName}/submit{queryString}";
        }

        public static Credentials GetCredentials(BlogConfig blogConfig)
        {
            return new Credentials()
            {
                User = blogConfig.RedditUser,
                Password = blogConfig.RedditPassword
            };
        }

        public static IList<Post> GetPostsForAutomaticRedditSubmission(IDocumentSession documentSession, DateTimeOffset currentDateTimeOffset)
        {
            return documentSession.QueryPublicPosts()
                .Where(x => x.PublishAt <= currentDateTimeOffset &&
                            x.TagsAsSlugs.Any(t => t == SendToRedditTag) &&
                            (x.Integration == null || 
                             x.Integration.Reddit == null ||
                             x.Integration.Reddit.Submitted == false))
                .OrderBy(x => x.PublishAt)
                .ToList();
        }

        public static IList<Post> GetPostsForManualRedditSubmission(IDocumentSession documentSession, DateTimeOffset currentDateTimeOffset)
        {
            return documentSession.QueryPublicPosts()
                .Where(x => x.PublishAt <= currentDateTimeOffset &&
                            x.TagsAsSlugs.Any(t => t == SendToRedditTag) &&
                            (x.Integration != null &&
                             x.Integration.Reddit != null ||
                             x.Integration.Reddit.PostSubmissions.Any(
                                 s => s.Status == Reddit.SubmissionStatus.CaptchaFailure || 
                                      s.Status == Reddit.SubmissionStatus.UnknownFailure)))
                .OrderBy(x => x.PublishAt)
                .ToList();
        }

        public static IList<string> ParseSubreddits(BlogConfig config)
        {
            if (string.IsNullOrEmpty(config.RedditSubredditsToSubmitToOnPublish))
                return new List<string>();

            return config.RedditSubredditsToSubmitToOnPublish
                .Split(',')
                .Select(x => x.Trim())
                .ToList();
        }

        private static string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            return "?" + string.Join("&", array);
        }
    }
}