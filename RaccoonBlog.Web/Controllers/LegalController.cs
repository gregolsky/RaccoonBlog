using Microsoft.AspNetCore.Mvc;

namespace RaccoonBlog.Web.Controllers
{
    public class LegalController : RaccoonController
    {
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