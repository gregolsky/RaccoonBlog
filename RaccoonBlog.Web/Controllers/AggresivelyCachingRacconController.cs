using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaccoonBlog.Web.Controllers
{
	public abstract partial class AggresivelyCachingRacconController : RaccoonController
	{
		IDisposable aggressivelyCacheFor;

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			aggressivelyCacheFor = RavenSession.Advanced.DocumentStore.AggressivelyCacheFor(CacheDuration);
		}

		protected abstract TimeSpan CacheDuration { get; }

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);

			if (aggressivelyCacheFor != null)
			{
				aggressivelyCacheFor.Dispose();
				aggressivelyCacheFor = null;
			}
		}
	}
}