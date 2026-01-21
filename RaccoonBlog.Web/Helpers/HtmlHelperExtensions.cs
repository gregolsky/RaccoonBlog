using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RaccoonBlog.Web.Models;

namespace RaccoonBlog.Web.Helpers
{
	public static class HtmlHelperExtensions
	{
		private static readonly HtmlString Empty = new HtmlString(string.Empty);

		public static HtmlString Glyphicon(this IHtmlHelper helper, string iconName)
		{
			return new HtmlString($"<i class=\"glyphicon glyphicon-{iconName}\"></i>");
		}

		public static bool IsSectionActive(this IHtmlHelper helper, string sectionTitle)
		{
			var sections = helper.ViewBag.Sections as List<Section>;
			if (sections == null)
				return false;

			var section = sections.FirstOrDefault(x => string.Equals(x.Title, sectionTitle, StringComparison.OrdinalIgnoreCase) && x.IsActive);
			if (section == null)
				return false;

			return true;
		}

		public static IHtmlContent RenderSection(this IHtmlHelper helper, string sectionTitle)
		{
			var sections = helper.ViewBag.Sections as List<Section>;
			if (sections == null)
				return null;

			var section = sections.FirstOrDefault(x => string.Equals(x.Title, sectionTitle, StringComparison.OrdinalIgnoreCase) && x.IsActive);
			if (section == null)
				return null;

			// ASP.NET Core Note: Html.Action() has been removed
			// Section actions should be rendered using ViewComponents or PartialViews instead
			// For now, just render the body content if available
			if (string.IsNullOrEmpty(section.Body) == false)
				return new HtmlString(section.Body);

			// TODO: Convert section actions to ViewComponents
			// Example: return await helper.PartialAsync($"Section/{section.ControllerName}/{section.ActionName}");
			return HtmlString.Empty;
		}

		public static string ConvertSectionTitleToId(this IHtmlHelper helper, string sectionTitle)
		{
			if (string.IsNullOrEmpty(sectionTitle))
				return string.Empty;

			return sectionTitle
				.Trim()
				.Replace(" ", "-")
				.ToLowerInvariant();
		}

		public static HtmlString Link(this IHtmlHelper helper, string text, string href, object htmlAttributes)
		{
			var tag = new TagBuilder("a");
			tag.InnerHtml.SetContent(text);

			if (string.IsNullOrEmpty(href) == false)
				tag.Attributes["href"] = href;

			var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
			foreach (var attribute in attributes)
			{
				var val = attribute.Value?.ToString();
				if (string.IsNullOrEmpty(val) == false)
					tag.Attributes[attribute.Key] = val;
			}

			using (var writer = new System.IO.StringWriter())
			{
				tag.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
				return new HtmlString(writer.ToString());
			}
		}

		public static IHtmlContent RenderTheme(this IHtmlHelper helper, string themeName)
		{
			// In ASP.NET Core, static file serving handles CSS
			// Theme CSS files should be referenced directly in views or via link tags
			if (string.IsNullOrEmpty(themeName))
				return HtmlString.Empty;

			// Return a link tag for the theme CSS
			return new HtmlString($"<link rel=\"stylesheet\" href=\"/css/custom/{themeName}.css\" />");
		}

		public static IHtmlContent RenderAdminTheme(this IHtmlHelper helper)
		{
			// Return a link tag for the admin theme CSS
			return new HtmlString("<link rel=\"stylesheet\" href=\"/admin/css/admin.styles.css\" />");
		}
	}
}