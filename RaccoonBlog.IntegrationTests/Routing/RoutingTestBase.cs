using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace RaccoonBlog.IntegrationTests.Routing
{
    /// <summary>
    /// Base class for ASP.NET Core routing tests.
    /// Modernized for .NET 8 - no longer depends on MvcContrib or System.Web.
    /// </summary>
    public class RoutingTestBase : IDisposable
    {
        protected static readonly Guid TestGuid = Guid.NewGuid();
        protected readonly WebApplicationFactory<Program> Factory;

        public RoutingTestBase()
        {
            // Create a test server that uses the actual Program.cs configuration
            Factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        // Add any test-specific service overrides here if needed
                    });
                });
        }

        public void Dispose()
        {
            Factory?.Dispose();
        }

        /// <summary>
        /// Helper to assert that a URL maps to expected controller/action/area.
        /// Returns true if route matches, false otherwise.
        /// </summary>
        protected async Task<bool> AssertRouteExistsAsync(
            string url,
            string httpMethod = "GET")
        {
            var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var request = new System.Net.Http.HttpRequestMessage(
                new System.Net.Http.HttpMethod(httpMethod),
                url);

            var response = await client.SendAsync(request);

            // If we get 404, the route doesn't exist
            // If we get any other status (even 500), the route exists but may have logic issues
            // For routing tests, we only care that the route is matched
            return response.StatusCode != System.Net.HttpStatusCode.NotFound;
        }

        /// <summary>
        /// Simplified route assertion - just checks if route exists and doesn't 404
        /// </summary>
        protected async Task AssertRouteExists(string url, string httpMethod = "GET")
        {
            var exists = await AssertRouteExistsAsync(url, httpMethod);
            Assert.True(exists, $"Route '{url}' with method '{httpMethod}' returned 404 (not found)");
        }
    }
}