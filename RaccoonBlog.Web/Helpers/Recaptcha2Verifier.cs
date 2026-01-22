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
        private readonly HttpClient _httpClient;
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public Recaptcha2Verifier(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<CaptchaVerificationResult> VerifyResponse(string token, string secret)
        {
            if (string.IsNullOrEmpty(token)) return CaptchaVerificationResult.Error("Token is empty");

            var content = new FormUrlEncodedContent(new[] {
            new KeyValuePair<string, string>("secret", secret),
            new KeyValuePair<string, string>("response", token)
        });

            try
            {
                var response = await _httpClient.PostAsync("/recaptcha/api/siteverify", content);
                response.EnsureSuccessStatusCode();
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());

                if (json["success"]?.Value<bool>() == true) return CaptchaVerificationResult.Valid;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Captcha API error");
            }
            return CaptchaVerificationResult.Error("Captcha verification failed.");
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