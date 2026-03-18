using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Controllers
{
	public partial class SearchController : RaccoonController
	{
		private readonly IConfiguration _configuration;

		public SearchController(IConfiguration configuration, IDocumentStore documentStore, IDocumentSession ravenSession) : base(documentStore, ravenSession)
        {
			_configuration = configuration;
		}

		private string GoogleCustomSearchId => _configuration["Raccoon:GoogleCustomSearch:Id"];

		public virtual IActionResult SearchResult(string q)
		{
			ViewBag.GoogleCustomSearchId = GoogleCustomSearchId;
			ViewBag.SearchTerm = q;
			return View((object)q);
		}
	}
}