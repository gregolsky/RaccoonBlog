using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Helpers.Results;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Infrastructure.Indexes;
using Raven.Client.Documents.Linq;

namespace RaccoonBlog.Web.Controllers
{
	public class PostsApiController : AggresivelyCachingRacconController
	{
		protected override TimeSpan CacheDuration => TimeSpan.FromMinutes(3);

		public virtual IActionResult GetPostsByTags(int count = 10) // ASP.NET Core: JsonNetResult ? IActionResult
		{
			if (count > 25)
				throw new InvalidOperationException("Count can be 25 maximum");

			var result =  RavenSession.Query<Posts_ByTag.Query, Posts_ByTag>()
			                         .Where(x => x.Tags.ContainsAny(new[] {"raven", "ravendb"}) && x.PublishAt < DateTimeOffset.Now.AsMinutes())
			                         .OrderByDescending(x => x.PublishAt)
			                         .Take(count)
			                         .Select(x => new
			                          {
				                          x.Title,
				                          x.PublishAt,
				                          x.Id
			                          })
			                         .ToList()  ;
			return Json(result); // ASP.NET Core: Json() returns IActionResult
		}
	}
}