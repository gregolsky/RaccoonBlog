using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Controllers
{
    public class LegalController : RaccoonController
    {
        public LegalController(IDocumentStore documentStore, IDocumentSession ravenSession)
: base(documentStore, ravenSession)
        {
        }
        [HttpGet]
        [Route("privacy-policy")]
        public virtual IActionResult PrivacyPolicy()
        {
            return View("PrivacyPolicy");
        }

        [HttpGet]
        [Route("terms")]
        public virtual IActionResult Terms()
        {
            return View("Terms");
        }
    }
}