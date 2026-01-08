// -----------------------------------------------------------------------
//  <copyright file="SocialController.cs" company="RavenDB LTD">
//      Copyright (c) RavenDB LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Models;

namespace RaccoonBlog.Web.Controllers
{
	public partial class SocialController : RaccoonController
	{
		public virtual IActionResult Login(string provider, string redirectUrl)
		{
			// Request a redirect to the external login provider
			var properties = new AuthenticationProperties
			{
				RedirectUri = Url.Action("ExternalLoginCallback", "Social", new { ReturnUrl = redirectUrl })
			};
			return Challenge(properties, provider);
		}

		private const string XsrfKey = "XsrfId";

		private static void SetCommenterValuesFromResponse(ClaimsPrincipal principal, Commenter commenter)
		{
			var emailClaim = principal.FindFirst(ClaimTypes.Email) ??
				principal.FindFirst("email");
			var nameClaim = principal.FindFirst(ClaimTypes.Name);
			var urlClaim = principal.FindFirst("urn:google:profile");

			if (string.IsNullOrWhiteSpace(commenter.Email) && emailClaim != null && string.IsNullOrWhiteSpace(emailClaim.Value) == false)
				commenter.Email = emailClaim.Value;

			if (string.IsNullOrWhiteSpace(commenter.Name) && nameClaim != null && string.IsNullOrWhiteSpace(nameClaim.Value) == false)
				commenter.Name = nameClaim.Value;

			if (string.IsNullOrWhiteSpace(commenter.Url) && urlClaim != null && string.IsNullOrWhiteSpace(urlClaim.Value) == false)
				commenter.Url = urlClaim.Value;
		}
		
		[AllowAnonymous]
		public virtual async Task<IActionResult> ExternalLoginCallback(string returnUrl)
		{
			Uri returnUri;
			Uri.TryCreate(returnUrl, UriKind.Absolute, out returnUri);

			var authenticateResult = await HttpContext.AuthenticateAsync();
			if (!authenticateResult.Succeeded || returnUri == null)
			{
				return returnUri != null ? (IActionResult)Redirect(returnUri.AbsoluteUri) : RedirectToRoute("homepage");
			}

			var principal = authenticateResult.Principal;
			var providerKey = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var loginProvider = principal.FindFirst("LoginProvider")?.Value ?? 
				principal.Identity?.AuthenticationType ?? "Unknown";

			if (string.IsNullOrEmpty(providerKey))
			{
				return returnUri != null ? (IActionResult)Redirect(returnUri.AbsoluteUri) : RedirectToRoute("homepage");
			}

			var claimedIdentifier = providerKey + "@" + loginProvider;
			var commenter = RavenSession.Query<Commenter>()
								.FirstOrDefault(c => c.OpenId == claimedIdentifier) ?? new Commenter
																					   {
																						   Key = Guid.NewGuid(),
																						   OpenId = claimedIdentifier,
																					   };

			SetCommenterValuesFromResponse(principal, commenter);

			CommenterUtil.SetCommenterCookie(Response, commenter.Key.MapTo<string>());
			RavenSession.Store(commenter);

			return Redirect(returnUri.AbsoluteUri);
		}
	}
}
