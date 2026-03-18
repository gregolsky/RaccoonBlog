using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaccoonBlog.Web.Helpers.Attributes
{
	/// <summary>
	/// Attribute to restrict action to AJAX calls only
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class AjaxOnlyAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var request = filterContext.HttpContext.Request;
			
			// Check if this is an AJAX request
			var isAjax = request.Headers["X-Requested-With"] == "XMLHttpRequest";
			
			if (!isAjax)
			{
				filterContext.Result = new NotFoundObjectResult("Only Ajax calls are permitted.");
			}
		}
	}
}