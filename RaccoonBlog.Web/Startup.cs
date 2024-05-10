using Microsoft.Owin;

using Owin;

using RaccoonBlog.Web;
using RaccoonBlog.Web.Infrastructure;

[assembly: OwinStartup(typeof(Startup))]
namespace RaccoonBlog.Web
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseForwardedHeaders();
			ConfigureAuth(app);
		}
	}
}