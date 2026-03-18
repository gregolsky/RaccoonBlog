using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;

namespace RaccoonBlog.Web.Controllers
{
	public partial class LegacyPostController : RaccoonController
	{
        public LegacyPostController(IDocumentStore documentStore, IDocumentSession ravenSession)
: base(documentStore, ravenSession)
        {
        }
        public virtual IActionResult RedirectLegacyPost(int year, int month, int day, string slug)
		{
			// attempt to find a post with match slug in the given date, but will back off the exact date if we can't find it
			var post = RavenSession.Query<Post>()
						.WhereIsPublicPost()
						.FirstOrDefault(p => p.LegacySlug == slug && (p.PublishAt.Year == year && p.PublishAt.Month == month && p.PublishAt.Day == day)) ??
					  RavenSession.Query<Post>()
						.WhereIsPublicPost()
						.FirstOrDefault(p => p.LegacySlug == slug && p.PublishAt.Year == year && p.PublishAt.Month == month) ??
					 RavenSession.Query<Post>()
						.WhereIsPublicPost()
						.FirstOrDefault(p => p.LegacySlug == slug && p.PublishAt.Year == year) ??
					 RavenSession.Query<Post>()
						.WhereIsPublicPost()
						.FirstOrDefault(p => p.LegacySlug == slug);

			if (post == null) 
			{
				return NotFound();
			}

			var postReference = post.MapTo<PostReference>();
			return RedirectToActionPermanent("Details", "PostDetails", new { Id = postReference.DomainId, postReference.Slug });
		}

		public virtual IActionResult RedirectLegacyArchive(int year, int month, int day)
		{
			return RedirectToActionPermanent("Archive", "Posts", new { year, month, day });
		}
	}
}
