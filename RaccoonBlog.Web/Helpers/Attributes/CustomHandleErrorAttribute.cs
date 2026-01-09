using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

namespace RaccoonBlog.Web.Helpers.Attributes
{
	public class CustomHandleErrorAttribute : ExceptionFilterAttribute
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public override void OnException(ExceptionContext context)
		{
			Log.Error(context.Exception, "Unexpected error occurred.");

			// Check if this is an AJAX request
			var isAjaxRequest = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

			if (isAjaxRequest)
			{
				base.OnException(context);
				return;
			}

			// In ASP.NET Core, custom error handling is typically done via middleware
			// But we can still redirect to error page from filter
			var env = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
			if (env != null && !env.IsDevelopment())
			{
				context.Result = new RedirectToActionResult("Error", "Error", null);
				context.ExceptionHandled = true;
			}

			base.OnException(context);
		}
	}
}