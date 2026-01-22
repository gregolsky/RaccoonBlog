using System.Threading.Tasks;
using Xunit;

namespace RaccoonBlog.IntegrationTests.Routing
{
	/// <summary>
	/// Tests for Admin area routes.
	/// Modernized for ASP.NET Core/.NET 8 - replaces MvcContrib test helpers.
	/// These tests verify that routes are properly configured and accessible.
	/// </summary>
	public class AdminAreaRoutesTests : RoutingTestBase
	{
		[Fact]
		public async Task LoginControllerRoutes_Index_Get()
		{
			await AssertRouteExists("/admin/login", "GET");
		}

		[Fact]
		public async Task LoginControllerRoutes_Index_Post()
		{
			await AssertRouteExists("/admin/login", "POST");
		}

		[Fact]
		public async Task LoginControllerRoutes_LogOut()
		{
			await AssertRouteExists("/admin/logout", "GET");
		}

		[Fact]
		public async Task LoginControllerRoutes_CurrentUser()
		{
			await AssertRouteExists("/admin/currentuser", "GET");
		}

		[Fact]
		public async Task UsersController_Index()
		{
			await AssertRouteExists("/admin/users", "GET");
		}

		[Fact]
		public async Task UsersController_Add()
		{
			await AssertRouteExists("/admin/users/add", "GET");
		}

		[Fact]
		public async Task UsersController_Edit()
		{
			await AssertRouteExists("/admin/users/4/edit", "GET");
		}

		[Fact]
		public async Task UsersController_ChangePassword_Get()
		{
			await AssertRouteExists("/admin/users/4/changepass", "GET");
		}

		[Fact]
		public async Task UsersController_ChangePassword_Post()
		{
			await AssertRouteExists("/admin/users/4/changepass", "POST");
		}

		[Fact]
		public async Task UsersController_SetActivation()
		{
			await AssertRouteExists("/admin/users/4/setactivation", "GET");
		}

		[Fact]
		public async Task UsersController_Update()
		{
			await AssertRouteExists("/admin/users/update", "POST");
		}

		[Fact]
		public async Task SectionsController_Index()
		{
			await AssertRouteExists("/admin/sections", "GET");
		}

		[Fact]
		public async Task SectionsController_Add()
		{
			await AssertRouteExists("/admin/sections/add", "GET");
		}

		[Fact]
		public async Task SectionsController_Edit()
		{
			await AssertRouteExists("/admin/sections/4/edit", "GET");
		}

		[Fact]
		public async Task SectionsController_Update()
		{
			await AssertRouteExists("/admin/sections/update", "POST");
		}

		[Fact]
		public async Task SectionsController_Delete()
		{
			await AssertRouteExists("/admin/sections/delete", "POST");
		}

		[Fact]
		public async Task SectionsController_SetPosition()
		{
			await AssertRouteExists("/admin/sections/4/setposition", "POST");
		}

		[Fact]
		public async Task SettingsController_Index()
		{
			await AssertRouteExists("/admin/configuration", "GET");
		}
	}
}