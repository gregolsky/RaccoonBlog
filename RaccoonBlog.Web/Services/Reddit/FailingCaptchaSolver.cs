using System;
using RedditSharp;

namespace RaccoonBlog.Web.Services.Reddit
{
    public class FailingCaptchaSolver : ICaptchaSolver
    {
        public CaptchaResponse HandleCaptcha(Captcha captcha)
        {
            return new CaptchaResponse();
        }
    }
}