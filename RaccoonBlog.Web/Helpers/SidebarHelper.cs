using Microsoft.AspNetCore.Http;

namespace RaccoonBlog.Web.Helpers
{
    public class SidebarHelper
    {
        private const int VisitCountMax = 10;

        public static bool ShouldShowSidebar(HttpRequest request)
        {
            var hasExplicitlyHidden = CookieJar.GetHideSidebar(request);
            if (hasExplicitlyHidden == true)
                return false;

            var isOnMainPage = request.Path == "/";
            if (isOnMainPage)
                return true;

            var visitCount = CookieJar.GetVisitCount(request).GetValueOrDefault();
            return visitCount < VisitCountMax;
        }
    }
}