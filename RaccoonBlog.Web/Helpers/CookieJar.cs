using Microsoft.AspNetCore.Http;

namespace RaccoonBlog.Web.Helpers
{
	public static class CookieJar
	{
		private static string GetRequestCookieText(HttpRequest request, string name)
		{
			if (request.Cookies.TryGetValue(name, out var value))
				return value;
			return null;
		}

		private static bool? GetRequestCookieBool(HttpRequest request, string name)
		{
			var value = GetRequestCookieText(request, name);
			if (string.IsNullOrEmpty(value))
				return null;

			if (bool.TryParse(value, out var result))
				return result;
			return null;
		}

		private static int? GetRequestCookieInt(HttpRequest request, string name)
		{
			var value = GetRequestCookieText(request, name);
			if (string.IsNullOrEmpty(value))
				return null;

			if (int.TryParse(value, out var result))
				return result;
			return null;
		}

		public static bool? GetHideSidebar(HttpRequest request) => GetRequestCookieBool(request, CookieNames.HideSidebar);
		public static int? GetVisitCount(HttpRequest request) => GetRequestCookieInt(request, CookieNames.VisitCount);

		private static class CookieNames
		{
			public const string HideSidebar = "hideSidebar";
			public const string VisitCount = "visitCount";
		}
	}
}