using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace RaccoonBlog.Web.Controllers
{
	// ASP.NET Core: CssController disabled - LESS compilation should be done at build time
	// Modern approach: Use build tools (npm, webpack, etc.) for CSS preprocessing
	// If needed, can use AspNetCore.SassCompiler or similar NuGet packages
	
	/*
	public partial class CssController : Controller
	{
		private readonly IWebHostEnvironment _env;

		public CssController(IWebHostEnvironment env)
		{
			_env = env;
		}

		public virtual ActionResult Merge(string[] files)
		{
			// This functionality should be moved to build-time CSS preprocessing
			// Using tools like: npm scripts, webpack, or ASP.NET Core bundling
			return NotFound("CSS merging moved to build-time processing");
		}
	}
	*/
}