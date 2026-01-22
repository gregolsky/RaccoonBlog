using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RaccoonBlog.IntegrationTests.Infrastructure;
using Raven.Client.Documents.Session;
using Xunit;

namespace RaccoonBlog.IntegrationTests.Web.Controllers
{
    /// <summary>
    /// Base class for controller integration tests using WebApplicationFactory.
    /// Provides proper DI support for .NET 8.
    /// </summary>
    public abstract class RaccoonControllerTests : IClassFixture<TestWebApplicationFactory>, IDisposable
    {
        protected readonly TestWebApplicationFactory Factory;

        protected RaccoonControllerTests(TestWebApplicationFactory factory)
        {
            Factory = factory;
        }

        /// <summary>
        /// Helper method to setup test data before running tests.
        /// </summary>
        protected void SetupData(Action<IDocumentSession> action)
        {
            Factory.SetupData(action);
        }

        /// <summary>
        /// Creates a service scope and executes an action with resolved dependencies.
        /// Use this for testing controllers with dependencies.
        /// </summary>
        protected void ExecuteWithScope(Action<IServiceProvider> action)
        {
            using var scope = Factory.Services.CreateScope();
            action(scope.ServiceProvider);
        }

        /// <summary>
        /// Gets a service from the test container.
        /// </summary>
        protected T GetService<T>() where T : notnull
        {
            return Factory.Services.GetRequiredService<T>();
        }

        /// <summary>
        /// Creates a new scoped service provider for testing.
        /// </summary>
        protected IServiceScope CreateScope()
        {
            return Factory.Services.CreateScope();
        }

        /// <summary>
        /// Helper method to properly initialize a controller with HttpContext and ControllerContext.
        /// Call this after creating a controller instance to ensure it has proper context for testing.
        /// </summary>
        protected void InitializeController(ControllerBase controller, IServiceProvider services)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData()
            };
        }

        public virtual void Dispose()
        {
            // Cleanup is handled by the factory
            GC.SuppressFinalize(this);
        }
    }
}