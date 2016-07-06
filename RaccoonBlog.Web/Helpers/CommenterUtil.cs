using System;
using System.Web;

namespace RaccoonBlog.Web.Helpers
{
	public static class CommenterUtil
	{
		public const string CommenterCookieName = "commenter";

		public static void SetCommenterCookie(HttpResponseBase response, string commenterKey)
		{
			var cookie = new HttpCookie(CommenterCookieName, commenterKey) {Expires = DateTime.Now.AddYears(1)};
			response.Cookies.Add(cookie);
		}

	    public static string GetCurrentCommenterKey(HttpRequestBase request)
	    {
	        return request.Cookies[CommenterCookieName]?.Value;
	    }

	    public static string GetCurrentCommenterKey()
	    {
	        return GetCurrentCommenterKey(new HttpRequestWrapper(HttpContext.Current.Request));
	    }
	}
}
