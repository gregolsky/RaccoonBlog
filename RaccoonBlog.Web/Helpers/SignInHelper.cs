using System.Security.Claims;
using System.Threading.Tasks;
using HibernatingRhinos.Loci.Common.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace RaccoonBlog.Web.Helpers
{
	public class SignInHelper
	{
		private readonly HttpContext httpContext;

		public SignInHelper(HttpContext httpContext)
		{
			this.httpContext = httpContext;
		}

		public async Task SignInAsync(LogOnModel logOn, bool isPersistent)
		{
			// Sign out any existing authentication
			await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			var claims = new[]
			{
				new Claim(ClaimTypes.Email, logOn.Login),
				new Claim(ClaimTypes.Name, logOn.Login)
			};

			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

			var authProperties = new AuthenticationProperties
			{
				IsPersistent = isPersistent || logOn.RememberMe,
				ExpiresUtc = logOn.RememberMe 
					? System.DateTimeOffset.UtcNow.AddDays(30) 
					: System.DateTimeOffset.UtcNow.AddHours(2)
			};

			await httpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				claimsPrincipal,
				authProperties);
		}

		public async Task SignOutAsync()
		{
			await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		}
	}
}