// ASP.NET Core: Area registration is no longer needed
// Areas are configured in Program.cs using app.MapControllerRoute with area pattern
// Keeping this file for reference only

/*
using System.Web.Mvc;

namespace RaccoonBlog.Web.Areas.Admin
{
	public class AdminAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Admin"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Admin_default",
				"admin/{controller}/{action}/{*id}",
				new { controller = "Posts", action = "Index", id = UrlParameter.Optional },
				new[] { "RaccoonBlog.Web.Areas.Admin.Controllers" }
			);
		}
	}
}
*/
