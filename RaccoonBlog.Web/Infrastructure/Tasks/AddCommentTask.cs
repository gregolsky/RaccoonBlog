using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using HibernatingRhinos.Loci.Common.Tasks;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.Services;
using RaccoonBlog.Web.ViewModels;
using System.Threading.Tasks;

namespace RaccoonBlog.Web.Infrastructure.Tasks
{
	public class AddCommentTask : BackgroundTask
	{
		private IAkismetService _akismetService;
        public AddCommentTask(IAkismetService akismetService) 
		{ 
			_akismetService = akismetService;
        }
		public class RequestValues
		{
			public string UserAgent { get; set; }
			public string UserHostAddress { get; set; }
			public bool IsAuthenticated { get; set; }
            public bool IsLocal { get; set; }
        }

		private readonly CommentInput commentInput;
		private readonly RequestValues requestValues;
		private readonly string postId;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServiceProvider serviceProvider;

		public AddCommentTask(CommentInput commentInput, RequestValues requestValues, string postId, IServiceProvider serviceProvider = null)
		{
			this.commentInput = commentInput;
			this.requestValues = requestValues;
			this.postId = postId;
			this.serviceProvider = serviceProvider;
            this._scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public override void Execute()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var akismetService = scope.ServiceProvider.GetRequiredService<IAkismetService>();

                var post = DocumentSession
                    .Include<Post>(x => x.AuthorId)
                    .Include(x => x.CommentsId)
                    .Load("posts/" + postId);

                var postAuthor = DocumentSession.Load<User>(post.AuthorId);
                var comments = DocumentSession.Load<PostComments>(post.CommentsId);

                var comment = new PostComments.Comment
                {
                    Id = comments.GenerateNewCommentId(),
                    Author = commentInput.Name,
                    Body = commentInput.Body,
                    CreatedAt = DateTimeOffset.Now,
                    Email = commentInput.Email,
                    Url = commentInput.Url,
                    Important = requestValues.IsAuthenticated,
                    UserAgent = requestValues.UserAgent,
                    UserHostAddress = requestValues.UserHostAddress,
                };

                comment.IsSpam = akismetService.CheckForSpam(comment);

                var commenter = DocumentSession.GetCommenter(commentInput.CommenterKey);
                if (commenter == null)
                {
                    Guid.TryParse(commentInput.CommenterKey, out var parsedKey);
                    commenter = new Commenter { Key = parsedKey };
                }
                SetCommenter(commenter, comment);

                if (requestValues.IsAuthenticated == false && comment.IsSpam)
                {
                    if (commenter.NumberOfSpamComments > 4)
                        return;
                    comments.Spam.Add(comment);
                }
                else
                {
                    post.CommentsCount++;
                    comments.Comments.Add(comment);
                }
               
                SendNewCommentEmail(post, comment, postAuthor, scope.ServiceProvider);
            }
        }

        private void SetCommenter(Commenter commenter, PostComments.Comment comment)
		{
			if (requestValues.IsAuthenticated)
				return;

			commentInput.MapPropertiesToInstance(commenter);
			commenter.IsTrustedCommenter = comment.IsSpam == false;

			if (comment.IsSpam)
				commenter.NumberOfSpamComments++;

			DocumentSession.Store(commenter);
			comment.CommenterId = commenter.Id;
		}

        private void SendNewCommentEmail(Post post, PostComments.Comment comment, User postAuthor, IServiceProvider localServiceProvider)
        {
            if (requestValues.IsAuthenticated)
                return;

            var viewModel = comment.MapTo<NewCommentEmailViewModel>();
            viewModel.PostId = post.GetIdForUrl();
            viewModel.PostTitle = WebUtility.HtmlDecode(post.Title);
            viewModel.PostSlug = SlugConverter.TitleToSlug(post.Title);
            viewModel.BlogName = DocumentSession.Load<BlogConfig>(BlogConfig.Key).Title;
            viewModel.Key = post.ShowPostEvenIfPrivate.ToString();
            viewModel.IsSpam = comment.IsSpam;
            viewModel.IpAddress = comment.UserHostAddress;
            viewModel.UserAgent = comment.UserAgent;

            var subject = string.Format("{2}Comment on: {0} from {1}", viewModel.PostTitle, viewModel.BlogName, viewModel.IsSpam ? "[DETECTED SPAM] " : string.Empty);

            TaskExecutor.ExcuteLater(new SendEmailTask(viewModel.Email, subject, "NewComment", postAuthor.Email, viewModel, localServiceProvider));
        }
    }
}