using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using RaccoonBlog.Web.Controllers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;

namespace RaccoonBlog.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public abstract partial class AdminController : RaccoonController
	{
		private IDisposable disableAggressiveCaching;

		protected AdminController(IDocumentStore documentStore, IDocumentSession ravenSession)
		: base(documentStore, ravenSession)
		{
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
            disableAggressiveCaching = DocumentStore.DisableAggressiveCaching();
			base.OnActionExecuting(filterContext);
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			using(disableAggressiveCaching)
				base.OnActionExecuted(filterContext);
		}
	}
}