using Microsoft.AspNetCore.Mvc;
using NLog;

namespace RaccoonBlog.Web.Controllers
{
    public partial class ErrorController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("error")]
        public virtual IActionResult Error()
        {
            HttpContext.Response.StatusCode = ViewBag.ErrorCode = 500;
            ViewBag.ErrorMessage = "error";

            return View("Error");
        }

        [HttpGet]
        [Route("error/404")]
        public virtual IActionResult Error404(string aspxerrorpath)
        {
            if (string.IsNullOrEmpty(aspxerrorpath) == false)
            {
                var sanitizedPath = System.Net.WebUtility.HtmlEncode(aspxerrorpath);
                Log.Warn("Could not find path: " + sanitizedPath);
            }

            HttpContext.Response.StatusCode = ViewBag.ErrorCode = 404;
            ViewBag.ErrorMessage = "not found";

            return View("Error");
        }
    }
}