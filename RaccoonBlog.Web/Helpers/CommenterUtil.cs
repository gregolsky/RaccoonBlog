using System;
using Microsoft.AspNetCore.Http;

namespace RaccoonBlog.Web.Helpers
{
	public static class CommenterUtil
	{
		public const string CommenterCookieName = "commenter";

		public static void SetCommenterCookie(HttpResponse response, string commenterKey)
		{
			var cookieOptions = new CookieOptions
			{
				Expires = DateTimeOffset.Now.AddYears(1),
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Lax
			};
			response.Cookies.Append(CommenterCookieName, commenterKey, cookieOptions);
		}
	}
}
