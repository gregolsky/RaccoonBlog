using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using RaccoonBlog.Web.Controllers;
using System;

namespace RaccoonBlog.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public abstract partial class AdminController : RaccoonController
	{
		private IDisposable disableAggressiveCaching;

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
            if (DocumentStore == null)
            {
                DocumentStore = filterContext.HttpContext.RequestServices.GetRequiredService<Raven.Client.Documents.IDocumentStore>();
            }

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