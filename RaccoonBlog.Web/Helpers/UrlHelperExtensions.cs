using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Html;
using JetBrains.Annotations;

namespace RaccoonBlog.Web.Helpers
{
	public static class UrlHelperExtensions
	{
		public static string PostUrl(this IUrlHelper url, HttpRequest request, string postId, string postSlug)
		{
			var fullUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
			fullUrl = fullUrl.TrimEnd('/');

			return $"{fullUrl}/{postId}/{postSlug}";
		}

		public static string AbsoluteAction(this IUrlHelper url, HttpRequest request, [AspMvcAction] string action, object routeValues)
		{
			return AbsoluteActionUtil(url, request, url.Action(action, routeValues));
		}

		public static string AbsoluteAction(this IUrlHelper url, HttpRequest request, [AspMvcAction] string action)
		{
			return AbsoluteActionUtil(url, request, url.Action(action));
		}

		public static string AbsoluteAction(this IUrlHelper url, HttpRequest request, [AspMvcAction] string action, [AspMvcController] string controller)
		{
			return AbsoluteActionUtil(url, request, url.Action(action, controller));
		}

		public static string AbsoluteAction(this IUrlHelper url, HttpRequest request, [AspMvcAction] string action, [AspMvcController] string controller, object routeValues)
		{
			return AbsoluteActionUtil(url, request, url.Action(action, controller, routeValues));
		}

		public static string RelativeToAbsolute(this IUrlHelper url, HttpRequest request, string relativeUrl)
		{
			return AbsoluteActionUtil(url, request, relativeUrl);
		}

		private static string AbsoluteActionUtil(IUrlHelper url, HttpRequest request, string relativeUrl)
		{
			var absoluteUrl = string.Format("{0}://{1}{2}",
				request.Scheme,
				request.Host,
				relativeUrl);

			return absoluteUrl;
		}


		public static IHtmlContent ActionLinkWithArray(this IUrlHelper url, [AspMvcAction] string action, [AspMvcController] string controller, object routeData)
		{
			string href = url.Action(action, controller, new {area = ""});

			var parameters = new List<string>();
			if (routeData != null)
			{
				var properties = routeData.GetType().GetProperties();
				foreach (var propertyInfo in properties)
				{
					var key = propertyInfo.Name;
					var value = propertyInfo.GetValue(routeData, null);
					var array = value as IEnumerable;
					if (array != null && !(array is string))
					{
						foreach (var val in array)
						{
							parameters.Add(string.Format("{0}={1}", key, val));
						}
					}
					else
					{
						parameters.Add(string.Format("{0}={1}", key, value));
					}
				}

			}

			string paramString = string.Join("&", parameters.ToArray());
			if (!string.IsNullOrEmpty(paramString))
			{
				href += "?" + paramString;
			}
			return new HtmlString(href);
		}
	}

	/// <summary>
	/// HTML Helper extensions for ASP.NET Core that replace Html.RenderAction
	/// </summary>
	public static class HtmlHelperActionExtensions
	{
		/// <summary>
		/// Renders an action result inline (replacement for Html.RenderAction)
		/// </summary>
		public static async Task RenderActionAsync(this IHtmlHelper html, string actionName, string controllerName, object routeValues = null)
		{
			if (html == null) throw new System.ArgumentNullException(nameof(html));
			if (actionName == null) throw new System.ArgumentNullException(nameof(actionName));

			var context = html.ViewContext.HttpContext;
			var serviceProvider = context.RequestServices;
			var actionInvoker = serviceProvider.GetService(typeof(IActionInvokerFactory)) as IActionInvokerFactory;
			var actionSelector = serviceProvider.GetService(typeof(IActionDescriptorCollectionProvider)) as IActionDescriptorCollectionProvider;

			if (actionInvoker == null || actionSelector == null)
			{
				// Fallback: render nothing
				return;
			}

			var routeData = new RouteData(html.ViewContext.RouteData);
			routeData.Values["controller"] = controllerName;
			routeData.Values["action"] = actionName;

			if (routeValues != null)
			{
				var properties = routeValues.GetType().GetProperties();
				foreach (var prop in properties)
				{
					var value = prop.GetValue(routeValues);
					routeData.Values[prop.Name] = value;
				}
			}

			var actionContext = new ActionContext(context, routeData, new ActionDescriptor());
			var invoker = actionInvoker.CreateInvoker(actionContext);

			if (invoker != null)
			{
				await invoker.InvokeAsync();
			}
		}
	}
}