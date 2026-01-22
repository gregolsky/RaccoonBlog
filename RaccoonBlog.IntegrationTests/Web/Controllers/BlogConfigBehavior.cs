using RaccoonBlog.Web.Areas.Admin.Controllers;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Models;
using RaccoonBlog.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Xunit;

namespace RaccoonBlog.IntegrationTests.Web.Controllers
{
    public class BlogConfigBehavior : RaccoonControllerTests
    {
        public BlogConfigBehavior(TestWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public void WhenTheBlogConfigIsAvailable_ThePropertyShouldReturnTheConfig()
        {
            var config = new BlogConfig { Title = "Test Config", Id = "Blog/Config" };
            SetupData(session => session.Store(config));

            BlogConfig configFromController = null;
            
            ExecuteWithScope(services =>
            {
                var signInHelper = services.GetRequiredService<SignInHelper>();
                var documentStore = services.GetRequiredService<IDocumentStore>();
                var session = services.GetRequiredService<IDocumentSession>();
                
                var controller = new LoginController(signInHelper, documentStore, session);
                InitializeController(controller, services);
                
                configFromController = controller.BlogConfig;
            });

            Assert.Equal(config.Title, configFromController.Title);
        }

        [Fact]
        public void WhenTheBlogConfigIsNotAvailable_AndWhenNotOnWelcomeController_ThePropertyShouldRedirectToRelativeWelcome()
        {
            ExecuteWithScope(services =>
            {
                var signInHelper = services.GetRequiredService<SignInHelper>();
                var documentStore = services.GetRequiredService<IDocumentStore>();
                var session = services.GetRequiredService<IDocumentSession>();
                
                var controller = new LoginController(signInHelper, documentStore, session);
                InitializeController(controller, services);
                
                controller.RouteData.Values.Add("controller", "notthewelcomecontroller");
                
                // Access BlogConfig - when it's null and not "welcome" controller, it calls Response.Redirect
                var _ = controller.BlogConfig;
                
                // In ASP.NET Core, Response.Redirect sets headers instead of throwing exception
                // Verify that Response.Redirect was called by checking the response status code and Location header
                Assert.Equal(302, controller.Response.StatusCode);
                Assert.True(controller.Response.Headers.ContainsKey("Location"));
                Assert.Contains("welcome", controller.Response.Headers["Location"].ToString().ToLower());
            });
        }

        [Fact]
        public void WhenTheBlogConfigIsNotAvailable_AndWhenOnWelcomeController_ThePropertyShouldNotRedirect()
        {
            ExecuteWithScope(services =>
            {
                var signInHelper = services.GetRequiredService<SignInHelper>();
                var documentStore = services.GetRequiredService<IDocumentStore>();
                var session = services.GetRequiredService<IDocumentSession>();
                
                var controller = new LoginController(signInHelper, documentStore, session);
                InitializeController(controller, services);
                
                controller.RouteData.Values.Add("controller", "welcome");
                
                // Access BlogConfig - when controller is "welcome", it should return null without redirect
                var config = controller.BlogConfig;
                
                // When BlogConfig doesn't exist and controller is "welcome", it should return null
                Assert.Null(config);
                
                // Verify no redirect occurred
                Assert.NotEqual(302, controller.Response.StatusCode);
                Assert.False(controller.Response.Headers.ContainsKey("Location"));
            });
        }
    }
}