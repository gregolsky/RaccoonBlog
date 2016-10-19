using System.Linq;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;
using System.Web.Mvc;
using RaccoonBlog.Web.Infrastructure;

namespace RaccoonBlog.Web.Controllers
{
	public partial class LegacyPostController : RaccoonController
	{
		public virtual ActionResult RedirectLegacyPost(int year, int month, int day, string slug)
		{
			//// attempt to find a post with match slug in the given date, but will back off the exact date if we can't find it
			var post =  
                QueryPublicPosts() 
					.FirstOrDefault(p => p.LegacySlug == slug && (p.PublishAt.Year == year && p.PublishAt.Month == month && p.PublishAt.Day == day)) ??
                QueryPublicPosts()
                    .FirstOrDefault(p => p.LegacySlug == slug && p.PublishAt.Year == year && p.PublishAt.Month == month) ??
                QueryPublicPosts()
                    .FirstOrDefault(p => p.LegacySlug == slug && p.PublishAt.Year == year) ??
                QueryPublicPosts()
                    .FirstOrDefault(p => p.LegacySlug == slug);

			if (post == null) 
			{
				return HttpNotFound();
			}

			var postReference = post.MapTo<PostReference>();
			return RedirectToActionPermanent("Details", "PostDetails", new { Id = postReference.DomainId, postReference.Slug });
		}

		public virtual ActionResult RedirectLegacyArchive(int year, int month, int day)
		{
			return RedirectToActionPermanent("Archive", "Posts", new { year, month, day });
		}
	}
}
