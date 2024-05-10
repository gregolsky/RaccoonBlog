using System;
using System.Linq;
using System.Web;
using Owin;

namespace RaccoonBlog.Web.Infrastructure
{
    public static class ForwardedHeadersExtensions
    {
        private const string ForwardedHeadersAdded = "ForwardedHeadersAdded";

        /// <summary>
        /// Checks for the presence of <c>X-Forwarded-For</c> and <c>X-Forwarded-Proto</c> headers, and if present updates the properties of the request with those headers' details.
        /// </summary>
        /// <remarks>
        /// This extension method is needed for operating our website on an HTTP connection behind a proxy which handles SSL hand-off. Such a proxy adds the <c>X-Forwarded-For</c>
        /// and <c>X-Forwarded-Proto</c> headers to indicate the nature of the client's connection.
        /// </remarks>
        public static IAppBuilder UseForwardedHeaders(this IAppBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // No need to add more than one instance of this middleware to the pipeline.
            if (!app.Properties.ContainsKey(ForwardedHeadersAdded))
            {
                app.Properties[ForwardedHeadersAdded] = true;

                app.Use(async (context, next) =>
                {
                    var request = context.Request;

                    if (request.Scheme != Uri.UriSchemeHttps && String.Equals(request.Headers["X-Forwarded-Proto"],
                            Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    {
                        var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                        var serverVars = httpContext.Request.ServerVariables;
                        serverVars["HTTPS"] = "on";
                        serverVars["SERVER_PORT_SECURE"] = "1";
                        serverVars["SERVER_PORT"] = "443";
                        if (request.Headers.TryGetValue("X-Forwarded-For", out var xForwardedFor)
                            && xForwardedFor.Length > 0)
                        {
                            serverVars["REMOTE_ADDR"] = xForwardedFor[0];
                        }
                    }

                    await next.Invoke().ConfigureAwait(false);
                });
            }

            return app;
        }
    }
}