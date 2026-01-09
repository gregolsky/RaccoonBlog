using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RaccoonBlog.Web.Controllers
{
	public partial class SearchController : RaccoonController
	{
		private readonly IConfiguration _configuration;

		public SearchController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		private string GoogleCustomSearchId => _configuration["Raccoon:GoogleCustomSearch:Id"]; // ASP.NET Core: Use IConfiguration

		public virtual IActionResult SearchResult(string q)
		{
			ViewBag.GoogleCustomSearchId = GoogleCustomSearchId;
			ViewBag.SearchTerm = q;
			return View((object)q);
		}
	}
}