using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace RaccoonBlog.Web.Helpers
{
    public class Recaptcha2Helper
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContext;
        private readonly Recaptcha2Verifier _verifier;
        public const string ModelStateErrorKey = "CaptchaNotValid";

        public Recaptcha2Helper(IConfiguration config, IHttpContextAccessor httpContext, Recaptcha2Verifier verifier)
        {
            _config = config;
            _httpContext = httpContext;
            _verifier = verifier;
        }

        public async Task<bool> Validate(ModelStateDictionary modelState)
        {
            var secret = _config["Recaptcha:Secret"];
            var token = _httpContext.HttpContext?.Request.Form["g-recaptcha-response"].ToString();

            var result = await _verifier.VerifyResponse(token, secret);
            if (result.IsValid) return true;

            modelState.AddModelError(ModelStateErrorKey, result.ErrorMessage);
            return false;
        }

        public IHtmlContent ScriptRef() => new HtmlString("<script src='https://www.google.com/recaptcha/api.js'></script>");
        public IHtmlContent Widget() => new HtmlString($"<div class='g-recaptcha' data-sitekey='{_config["Recaptcha:SiteKey"]}'></div>");
    }
}