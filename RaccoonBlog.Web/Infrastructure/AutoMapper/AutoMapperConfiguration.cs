using AutoMapper;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;
using System;

namespace RaccoonBlog.Web.Infrastructure.AutoMapper
{
	public class AutoMapperConfiguration : Profile
	{
	    public AutoMapperConfiguration()
	    {
	        // Global type converters - use ConvertUsing with lambda for AutoMapper 12.x compatibility
	        // This avoids DI resolution issues with the converters
	        CreateMap<string, IHtmlContent>().ConvertUsing((src, dest, context) => 
	            new HtmlString(src ?? string.Empty));
	        
	        CreateMap<Guid, string>().ConvertUsing((src, dest, context) => 
	            src.ToString());
	        
	        CreateMap<DateTimeOffset, DateTime>().ConvertUsing((src, dest, context) => 
	            src.DateTime);

            CreateMap<HttpRequest, RaccoonBlog.Web.Infrastructure.Tasks.AddCommentTask.RequestValues>()
				.ForMember(dest => dest.UserHostAddress, opt => opt.MapFrom(src => src.HttpContext.Connection.RemoteIpAddress.ToString()))
				.ForMember(dest => dest.UserAgent, opt => opt.MapFrom(src => src.Headers["User-Agent"].ToString()))
				.ForMember(dest => dest.IsLocal, opt => opt.MapFrom(src =>
					src.HttpContext.Connection.RemoteIpAddress.Equals(src.HttpContext.Connection.LocalIpAddress)));
        }
	}
}