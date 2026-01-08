using System;
using System.Text.RegularExpressions;
using System.Web;
using Markdig;

namespace RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers
{
	public class MarkdownResolver
	{
		private static readonly Regex Backticks = new Regex(@"^```+\s*$", RegexOptions.Multiline);

		public static string Resolve(string inputBody)
		{
			var html = FormatMarkdown(inputBody);
			return html;
		}

		private static string NormalizeContent(string content)
		{
			return Backticks.Replace(content, "~~~");
		}

		private static string FormatMarkdown(string content)
		{
			var normalized = NormalizeContent(content);

			string result;

			try
			{
				// Use Markdig pipeline with advanced features
				var pipeline = new MarkdownPipelineBuilder()
					.UseAdvancedExtensions()  // Includes tables, task lists, etc.
					.UseSoftlineBreakAsHardlineBreak()
					.Build();

				result = Markdown.ToHtml(normalized, pipeline);
			}
			catch (Exception)
			{
				result = string.Format("<pre>{0}</pre>", HttpUtility.HtmlEncode(content));
			}

			return result;
		}
	}
}