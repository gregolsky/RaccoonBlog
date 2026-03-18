using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HibernatingRhinos.Loci.Common.Extensions
{
	public static class Html5Helpers
	{
		public static IHtmlContent Html5DateTag(this IHtmlHelper html, DateTimeOffset timestamp)
		{
			return new HtmlString(string.Format(@"<time datetime=""{0}"">{1}</time>", timestamp.ToString("yyyy-MM-ddTHH:mm"), timestamp.ToString("dddd, dd MMMM yyyy")));
		}

		public static IHtmlContent Html5DateTimeTag(this IHtmlHelper html, DateTimeOffset timestamp)
		{
			return new HtmlString(string.Format(@"<time datetime=""{0}"">{1}</time>", timestamp.ToString("yyyy-MM-ddTHH:mm"), timestamp.ToString("dddd, dd MMMM yyyy, HH:mm")));
		}

		public static IHtmlContent Html5MinutesAgoTag(this IHtmlHelper html, DateTimeOffset timestamp)
		{
			return new HtmlString(string.Format(@"<time datetime=""{0}"">{1}</time>", timestamp.ToString("yyyy-MM-ddTHH:mm"),
				DateTimeOffset.Now.Subtract(timestamp).ToReadableString()
				));
		}
	}
}