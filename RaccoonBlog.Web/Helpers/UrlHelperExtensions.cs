using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
}