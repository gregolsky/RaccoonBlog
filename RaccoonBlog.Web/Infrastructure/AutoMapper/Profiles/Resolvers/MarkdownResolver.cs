using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Markdig;
using HtmlAgilityPack;

namespace RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers
{
	public class MarkdownResolver
	{
		private static readonly Regex Backticks = new Regex(@"^```+\s*$", RegexOptions.Multiline);

		public static string Resolve(string inputBody)
		{
			var html = FormatMarkdown(inputBody);
			var sanitized = SanitizeHtml(html);
			return sanitized;
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

		private static string SanitizeHtml(string html)
		{
			// Allowed HTML tags - whitelist approach
			var allowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { 
				"p", "br", "strong", "em", "u", "b", "i", 
				"a", "ul", "ol", "li", 
				"code", "pre", "kbd", 
				"blockquote", 
				"h1", "h2", "h3", "h4", "h5", "h6",
				"hr",
				"dl", "dt", "dd",
				"del", "s", "strike"
			};

			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			// Remove all script tags and dangerous elements
			var dangerousNodes = doc.DocumentNode.SelectNodes("//script|//iframe|//object|//embed|//applet|//form|//input|//button|//select|//textarea|//meta|//link|//style|//img");
			if (dangerousNodes != null)
			{
				foreach (var node in dangerousNodes.ToList())
				{
					node.Remove();
				}
			}

			// Remove javascript: protocol from all href and src attributes
			var nodesWithDangerousAttrs = doc.DocumentNode.SelectNodes("//*[@href or @src]");
			if (nodesWithDangerousAttrs != null)
			{
				foreach (var node in nodesWithDangerousAttrs.ToList())
				{
					var href = node.GetAttributeValue("href", "");
					var src = node.GetAttributeValue("src", "");
					if (href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
					    src.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
					{
						node.Remove();
					}
				}
			}

			// Remove all tags NOT in the whitelist by unwrapping them
			var allNodes = doc.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element).ToList();
			foreach (var node in allNodes)
			{
				if (!allowedTags.Contains(node.Name))
				{
					// Unwrap: move children up and remove the tag
					var parent = node.ParentNode;
					if (parent != null)
					{
						// Move all child nodes to parent before this node
						var children = node.ChildNodes.ToList();
						foreach (var child in children)
						{
							parent.InsertBefore(child, node);
						}
						// Remove the now-empty wrapper node
						parent.RemoveChild(node);
					}
				}
			}

			return doc.DocumentNode.OuterHtml;
		}
	}
}