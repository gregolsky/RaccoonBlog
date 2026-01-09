using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RaccoonBlog.Web.Helpers
{
    public class Recaptcha2Helper
    {
        // TODO: Add proper recaptcha configuration to ConfigurationHelper
        private static string RecaptchaSecret => string.Empty;

        private static string SiteKey => string.Empty; // TODO: Add to ConfigurationHelper if needed

        public const string ModelStateErrorKey = "CaptchaNotValid";

        public static async Task<bool> Validate(HttpContext httpContext, ModelStateDictionary modelState)
        {
            // For now, skip recaptcha validation during migration
            // TODO: Implement proper recaptcha validation with IConfiguration
            return await Task.FromResult(true);
            
            /*
            var result = await Recaptcha2Verifier.VerifyResponse(httpContext, RecaptchaSecret);
            if (result.IsValid)
            {
                return true;
            }

            modelState.AddModelError(ModelStateErrorKey, result.ErrorMessage);
            return false;
            */
        }

        // Overload for backward compatibility - uses default validation skip
        public static async Task<bool> Validate(ModelStateDictionary modelState)
        {
            return await Task.FromResult(true);
        }

        public static IHtmlContent ScriptRef()
        {
            return new HtmlString("<script src='https://www.google.com/recaptcha/api.js'></script>");
        }

        public static IHtmlContent Widget()
        {
            var result = $"<div class='g-recaptcha' data-sitekey='{SiteKey}'></div>";
            return new HtmlString(result);
        }
    }
}