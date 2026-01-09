using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using NLog;

namespace RaccoonBlog.Web.Helpers
{
    public class Recaptcha2Verifier
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private const string RecaptchaResponseFieldName = "g-recaptcha-response";

        public static async Task<CaptchaVerificationResult> VerifyResponse(HttpContext httpContext, string recaptchaSecret)
        {
            if (httpContext?.Request?.HasFormContentType != true)
            {
                return CaptchaVerificationResult.Error("Captcha response not supplied.");
            }

            var form = await httpContext.Request.ReadFormAsync();
            var response = form[RecaptchaResponseFieldName].ToString();
            
            if (string.IsNullOrEmpty(response))
            {
                return CaptchaVerificationResult.Error("Captcha response not supplied.");
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.google.com"),
                Timeout = TimeSpan.FromSeconds(30)
            };

            var formContent = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("secret", recaptchaSecret),
                    new KeyValuePair<string, string>("response", response)
                });

            HttpResponseMessage apiResponse = await httpClient.PostAsync(
                "/recaptcha/api/siteverify", formContent).ConfigureAwait(false);

            string responseContent = await apiResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                JObject responseObj = JObject.Parse(responseContent);

                if (responseObj["success"].Value<bool>())
                {
                    return CaptchaVerificationResult.Valid;
                }
            }
            catch (Exception err)
            {
                _log.Error(err, "Error validating captcha.");
            }

            return CaptchaVerificationResult.Error("Captcha response is invalid. Please try again.");
        }
    }

    public class CaptchaVerificationResult
    {
        public bool IsValid { get; set; }

        public string ErrorMessage { get; set; }

        public static CaptchaVerificationResult Valid => new CaptchaVerificationResult()
        {
            IsValid = true
        };

        public static CaptchaVerificationResult Error(string msg)
        {
            return new CaptchaVerificationResult
            {
                ErrorMessage = msg
            };
        }
    }
}