using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using HibernatingRhinos.Loci.Common.Tasks;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Helpers.Validation;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Infrastructure.Tasks;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.Controllers
{
    using System.Collections.Generic;
    using RaccoonBlog.Web.Infrastructure.Indexes;
    using Raven.Client.Linq;

	public partial class PostDetailsController : RaccoonController
	{
		public virtual ActionResult Details(int id, string slug, Guid key)
		{
			var post = RavenSession
				.Include<Post>(x => x.CommentsId)
				.Include(x => x.AuthorId)
				.Load(id);

			if (post == null)
				return HttpNotFound();

			if (post.IsPublicPost(key) == false)
				return HttpNotFound();

            var vm = PreparePostViewModel(post);

		    HandleCommentAction(vm);

            if (vm.Post.Slug != slug)
				return RedirectToActionPermanent("Details", new {id, vm.Post.Slug});

			SetWhateverUserIsTrustedCommenter(vm);

			return View("Details", vm);
		}

	    private PostViewModel PreparePostViewModel(Post post)
	    {
	        SeriesInfo seriesInfo = GetSeriesInfo(post.Title);

	        var commentsDoc = RavenSession.Load<PostComments>(post.CommentsId) ?? 
                new PostComments();

	        var currentCommenterKey = CommenterUtil.GetCurrentCommenterKey();
	        var commentsModel = commentsDoc.Comments.OrderBy(x => x.CreatedAt)
                .Select(c =>
	            {
	                var commentModel = c.MapTo<PostViewModel.Comment>();
	                commentModel.Editable = IsCommentEditable(c, currentCommenterKey);
	                return commentModel;
	            })
                .ToList();

            var vm = new PostViewModel
	        {
	            Post = post.MapTo<PostViewModel.PostDetails>(),
	            Comments = commentsModel,
	            NextPost = RavenSession.GetNextPrevPost(post, true),
	            PreviousPost = RavenSession.GetNextPrevPost(post, false),
	            AreCommentsClosed = commentsDoc.AreCommentsClosed(post, BlogConfig.NumberOfDayToCloseComments),
	            SeriesInfo = seriesInfo
	        };

	        vm.Post.Author = RavenSession.Load<User>(post.AuthorId).MapTo<PostViewModel.UserDetails>();
	        return vm;
	    }

	    private bool IsCommentEditable(PostComments.Comment c, string currentCommenterKey)
	    {
	        if (currentCommenterKey != null && c.CommenterKey == currentCommenterKey)
	            return true;

	        var claims = User.Identity as ClaimsIdentity;
	        if (c.CommenterId == null && claims != null && claims.IsAuthenticated)
	        {
	            return claims.FindFirst(ClaimTypes.Email)?.Value == c.Email;
	        }

            return false;
	    }

	    [ValidateInput(false)]
		[HttpPost]
		public virtual ActionResult Comment(CommentInput input, int id, Guid key)
		{
		    if (ModelState.IsValid == false)
                return RedirectToAction("Details");

		    var post = RavenSession
		        .Include<Post>(x => x.CommentsId)
		        .Load(id);

		    if (post == null || post.IsPublicPost(key) == false)
		        return HttpNotFound();

		    var comments = RavenSession.Load<PostComments>(post.CommentsId);
		    if (comments == null)
		        return HttpNotFound();

		    var commenter = RavenSession.GetCommenter(input.CommenterKey);
		    if (commenter == null)
		    {
		        input.CommenterKey = Guid.NewGuid();
		    }

		    ValidateCommentsAllowed(post, comments);
		    ValidateCaptcha(input, commenter);

		    if (ModelState.IsValid == false)
		        return PostingCommentFailed(post, input, key);

            input.CreatedAt = DateTimeOffset.Now;

		    TaskExecutor.ExcuteLater(new AddCommentTask(input, Request.MapTo<AddCommentTask.RequestValues>(), id));

		    CommenterUtil.SetCommenterCookie(Response, input.CommenterKey.MapTo<string>());

		    OutputCacheManager.RemoveItem(SectionController.NameConst, MVC.Section.ActionNames.List);

		    return PostingCommentSucceeded(post, input);
		}

	    private void HandleCommentAction(PostViewModel vm)
	    {
			var comment = TempData["comment-action"] as CommentInput;

	        if (comment == null)
	            return;

	        if (comment.Action == CommentInput.CommentAction.Update)
	        {
	            var commentModel = vm.Comments.First(x => x.CreatedAt == comment.CreatedAt.ToString() && x.Editable);
	            commentModel.Body = MarkdownResolver.Resolve(comment.Body);
	        }

	        if (comment.Action == null || comment.Action == CommentInput.CommentAction.Post)
	        {
                vm.Comments.Add(new PostViewModel.Comment
                {
                    CreatedAt = comment.CreatedAt.ToString(),
                    Author = comment.Name,
                    Body = MarkdownResolver.Resolve(comment.Body),
                    Id = -1,
                    Url = UrlResolver.Resolve(comment.Url),
                    Tooltip = "Comment by " + comment.Name,
                    EmailHash = EmailHashResolver.Resolve(comment.Email),
                    Editable = true
                });
            }
        }

	    private ActionResult PostingCommentSucceeded(Post post, CommentInput input)
		{
			const string successMessage = "Your comment will be posted soon. Thanks!";
			if (Request.IsAjaxRequest())
				return Json(new {Success = true, message = successMessage});

			TempData["comment-action"] = input;
			var postReference = post.MapTo<PostReference>();

			return Redirect(Url.Action("Details",
				new { Id = postReference.DomainId, postReference.Slug, key = post.ShowPostEvenIfPrivate }) + "#comments-form-location");
		}

		private void ValidateCommentsAllowed(Post post, PostComments comments)
		{
			if (comments.AreCommentsClosed(post, BlogConfig.NumberOfDayToCloseComments))
				ModelState.AddModelError("CommentsClosed", "This post is closed for new comments.");
			if (post.AllowComments == false)
				ModelState.AddModelError("CommentsClosed", "This post does not allow comments.");
		}

		private void ValidateCaptcha(CommentInput input, Commenter commenter)
		{
			if (Request.IsAuthenticated ||
			    (commenter != null && commenter.IsTrustedCommenter == true))
				return;

			if (RecaptchaValidatorWrapper.Validate(ControllerContext.HttpContext))
				return;

			ModelState.AddModelError("CaptchaNotValid",
			                         "You did not type the verification word correctly. Please try again.");
		}

		private ActionResult PostingCommentFailed(Post post, CommentInput input, Guid key)
		{
			if (Request.IsAjaxRequest())
				return Json(new {Success = false, message = ModelState.FirstErrorMessage()});

			var postReference = post.MapTo<PostReference>();
			var result = Details(postReference.DomainId, postReference.Slug, key);
			var model = result as ViewResult;
			if (model != null)
			{
				var viewModel = model.Model as PostViewModel;
				if (viewModel != null)
					viewModel.Input = input;
			}
			return result;
		}

		private void SetWhateverUserIsTrustedCommenter(PostViewModel vm)
		{
			if (Request.IsAuthenticated)
			{
				var user = RavenSession.GetCurrentUser();
				vm.Input = user.MapTo<CommentInput>();
				vm.IsTrustedCommenter = true;
				vm.IsLoggedInCommenter = true;
				return;
			}

			var commenterKey = CommenterUtil.GetCurrentCommenterKey();
			if (commenterKey == null) return;

			var commenter = RavenSession.GetCommenter(commenterKey);
			if (commenter == null)
			{
				vm.IsLoggedInCommenter = false;
				Response.Cookies.Set(new HttpCookie(CommenterUtil.CommenterCookieName) {Expires = DateTime.Now.AddYears(-1)});
				return;
			}

			vm.IsLoggedInCommenter = string.IsNullOrWhiteSpace(commenter.OpenId) == false;
			vm.Input = commenter.MapTo<CommentInput>();
			vm.IsTrustedCommenter = commenter.IsTrustedCommenter == true;
		}

        private SeriesInfo GetSeriesInfo(string title)
        {
            SeriesInfo seriesInfo = null;
	        string seriesTitle = TitleConverter.ToSeriesTitle(title);

            if (!string.IsNullOrEmpty(seriesTitle))
            {
                var series = RavenSession.Query<Posts_Series.Result, Posts_Series>()
                    .Where(x => x.Series.StartsWith(seriesTitle) && x.Count > 1)
                    .OrderByDescending(x => x.MaxDate)
                    .FirstOrDefault();

	            if (series == null) 
					return null;

                var postsInSeries = GetPostsForCurrentSeries(series);

                seriesInfo = new SeriesInfo
                {
                    SeriesId = series.SerieId,
                    SeriesTitle = seriesTitle,
                    PostsInSeries = postsInSeries
                };
            }

            return seriesInfo;
        }

        private IList<PostInSeries> GetPostsForCurrentSeries(Posts_Series.Result series)
        {
            IList<PostInSeries> postsInSeries = null;

            if (series != null)
            {
                postsInSeries = series
					.Posts
					.Select(s => new PostInSeries
					{
						Id = RavenIdResolver.Resolve(s.Id), 
						Slug = SlugConverter.TitleToSlug(s.Title), 
						Title = HttpUtility.HtmlDecode(TitleConverter.ToPostTitle(s.Title)),
						PublishAt = s.PublishAt
					})
					.OrderByDescending(p => p.PublishAt)
					.ToList();
            }

            return postsInSeries;
        }
	}
}