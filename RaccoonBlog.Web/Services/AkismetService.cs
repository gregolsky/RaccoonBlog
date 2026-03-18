using Joel.Net;
using Microsoft.Extensions.Configuration;
using RaccoonBlog.Web.Controllers;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Threading.Tasks;

namespace RaccoonBlog.Web.Services
{
    public interface IAkismetService
    {
        bool CheckForSpam(PostComments.Comment comment);
        void MarkHam(PostComments.Comment comment);
        void MarkSpam(PostComments.Comment comment);
    }

    public class AkismetService : IAkismetService
    {
        private readonly IDocumentSession _session;
        private readonly IConfiguration _configuration;

        public AkismetService(IDocumentSession session, IConfiguration configuration)
        {
            _session = session;
            _configuration = configuration;
        }

        private string AkismetKey => _session.Load<BlogConfig>(BlogConfig.Key)?.AkismetKey;

        private string BlogUrl => _configuration["AppSettings:MainUrl"];

        public bool CheckForSpam(PostComments.Comment comment)
        {
#if DEBUG 
            return false;
#endif
            var api = new Akismet(AkismetKey, BlogUrl, comment.UserAgent);
            if (!api.VerifyKey()) throw new Exception("Akismet API key invalid.");

            var akismetComment = new AkismetComment
            {
                Blog = BlogUrl,
                UserIp = comment.UserHostAddress,
                UserAgent = comment.UserAgent,
                CommentContent = comment.Body,
                CommentType = "comment",
                CommentAuthor = comment.Author,
                CommentAuthorEmail = comment.Email,
                CommentAuthorUrl = comment.Url,
            };

            return api.CommentCheck(akismetComment);
        }

        public void MarkHam(PostComments.Comment comment)
        {
            var api = new Akismet(AkismetKey, BlogUrl, comment.UserAgent);
            if (!api.VerifyKey()) throw new Exception("Akismet API key invalid.");

            var akismetComment = new AkismetComment
            {
                Blog = BlogUrl,
                UserIp = comment.UserHostAddress,
                UserAgent = comment.UserAgent,
                CommentContent = comment.Body,
                CommentType = "comment",
                CommentAuthor = comment.Author,
                CommentAuthorEmail = comment.Email,
                CommentAuthorUrl = comment.Url,
            };
#if !DEBUG
            api.SubmitHam(akismetComment);
#endif
        }

        public void MarkSpam(PostComments.Comment comment)
        {
            var api = new Akismet(AkismetKey, BlogUrl, comment.UserAgent);
            if (!api.VerifyKey()) throw new Exception("Akismet API key invalid.");

            var akismetComment = new AkismetComment
            {
                Blog = BlogUrl,
                UserIp = comment.UserHostAddress,
                UserAgent = comment.UserAgent,
                CommentContent = comment.Body,
                CommentType = "comment",
                CommentAuthor = comment.Author,
                CommentAuthorEmail = comment.Email,
                CommentAuthorUrl = comment.Url,
            };
#if !DEBUG
            api.SubmitSpam(akismetComment);
#endif
        }
    }
}
