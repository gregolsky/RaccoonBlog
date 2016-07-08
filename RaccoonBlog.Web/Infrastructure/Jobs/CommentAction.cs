using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HibernatingRhinos.Loci.Common.Tasks;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Infrastructure.Tasks;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.Services;
using RaccoonBlog.Web.ViewModels;
using Raven.Client;
using Raven.Client.Document;

namespace RaccoonBlog.Web.Infrastructure.Jobs
{
    public class CommentAction : JobBase
    {
		public class RequestValues
		{
			public string UserAgent { get; set; }
			public string UserHostAddress { get; set; }
			public bool IsAuthenticated { get; set; }
		}

		private readonly CommentInput _commentInput;
		private readonly RequestValues _requestValues;
		private readonly int _postId;

		public CommentAction(CommentInput commentInput, RequestValues requestValues, int postId)
		{
			_commentInput = commentInput;
			_requestValues = requestValues;
			_postId = postId;
		}

        protected override void Run(IDocumentSession session)
        {
			var post = session 
				.Include<Post>(x => x.AuthorId)
				.Include(x => x.CommentsId)
				.Load(_postId);
			var postAuthor = session.Load<User>(post.AuthorId);
			var comments = session.Load<PostComments>(post.CommentsId);

			var comment = new PostComments.Comment
			              	{
			              		Id = comments.GenerateNewCommentId(),
			              		Author = _commentInput.Name,
			              		Body = _commentInput.Body,
			              		CreatedAt = DateTimeOffset.Now,
			              		Email = _commentInput.Email,
			              		Url = _commentInput.Url,
			              		Important = _requestValues.IsAuthenticated, // TODO: Don't mark as important based on that
			              		UserAgent = _requestValues.UserAgent,
			              		UserHostAddress = _requestValues.UserHostAddress,
			              	};

			comment.IsSpam = AkismetService.CheckForSpam(comment);

			var commenter = session.GetCommenter(_commentInput.CommenterKey) ?? 
                new Commenter { Key = _commentInput.CommenterKey ?? Guid.Empty };
			SetCommenter(session, commenter, comment);

			if (_requestValues.IsAuthenticated == false && comment.IsSpam)
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

			SendNewCommentEmail(session, post, comment, postAuthor);
        }

		private void SetCommenter(IDocumentSession session, Commenter commenter, PostComments.Comment comment)
		{
			if (_requestValues.IsAuthenticated)
				return;

			_commentInput.MapPropertiesToInstance(commenter);
			commenter.IsTrustedCommenter = comment.IsSpam == false;

			if (comment.IsSpam)
				commenter.NumberOfSpamComments++;

			session.Store(commenter);
		    comment.CommenterKey = commenter.Key.MapTo<string>();
			comment.CommenterId = commenter.Id;
		}

		private void SendNewCommentEmail(IDocumentSession session, Post post, PostComments.Comment comment, User postAuthor)
		{
			if (_requestValues.IsAuthenticated)
				return; // we don't send email for authenticated users

			var viewModel = comment.MapTo<NewCommentEmailViewModel>();
			viewModel.PostId = RavenIdResolver.Resolve(post.Id);
			viewModel.PostTitle = HttpUtility.HtmlDecode(post.Title);
			viewModel.PostSlug = SlugConverter.TitleToSlug(post.Title);
			viewModel.BlogName = session.Load<BlogConfig>(BlogConfig.Key).Title;
			viewModel.Key = post.ShowPostEvenIfPrivate.MapTo<string>();
		    viewModel.IsSpam = comment.IsSpam;

			var subject = string.Format("{2}Comment on: {0} from {1}", viewModel.PostTitle, viewModel.BlogName, viewModel.IsSpam ? "[DETECTED SPAM] " : string.Empty);

			TaskExecutor.ExcuteLater(new SendEmailTask(viewModel.Email, subject, "NewComment", postAuthor.Email, viewModel));
		}
    }
}