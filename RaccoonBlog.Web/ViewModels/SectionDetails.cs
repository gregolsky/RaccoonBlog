using Microsoft.AspNetCore.Html;

namespace RaccoonBlog.Web.ViewModels
{
	public class SectionDetails
	{
		public string Title { get; set; }

		public IHtmlContent Body { get; set; }
		public string ControllerName { get; set; }
		public string ActionName { get; set; }

		public bool IsActionSection()
		{
			return Body == null || string.IsNullOrEmpty(Body.ToString());
		}
	}
}