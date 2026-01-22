using Microsoft.AspNetCore.Mvc.Filters;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;

namespace RaccoonBlog.Web.Controllers
{
	public abstract partial class AggresivelyCachingRacconController : RaccoonController
	{
		IDisposable aggressivelyCacheFor;
        public AggresivelyCachingRacconController(IDocumentStore documentStore, IDocumentSession ravenSession)
: base(documentStore, ravenSession)
        {
        }
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