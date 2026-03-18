using AutoMapper;

namespace RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles
{
	/// <summary>
	/// Base profile for AutoMapper configurations
	/// ASP.NET Core: UrlHelper is no longer available as static context
	/// Use dependency injection or direct URL construction instead
	/// </summary>
	public abstract class AbstractProfile : Profile
	{
		// ASP.NET Core: HttpContext.Current doesn't exist
		// UrlHelper should be injected where needed, not accessed statically
	}
}