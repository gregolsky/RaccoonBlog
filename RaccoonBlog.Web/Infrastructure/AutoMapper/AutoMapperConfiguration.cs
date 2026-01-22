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
	        // Note: String -> IHtmlContent mapping is in PostsViewModelMapperProfile to avoid duplication
	        
	        CreateMap<Guid, string>().ConvertUsing((src, dest, context) => 
	            src.ToString());
	        
	        CreateMap<DateTimeOffset, DateTime>().ConvertUsing((src, dest, context) => 
	            src.DateTime);

            CreateMap<HttpRequest, RaccoonBlog.Web.Infrastructure.Tasks.AddCommentTask.RequestValues>()
				.ForMember(dest => dest.UserHostAddress, opt => opt.MapFrom(src => src.HttpContext.Connection.RemoteIpAddress.ToString()))
				.ForMember(dest => dest.UserAgent, opt => opt.MapFrom(src => src.Headers["User-Agent"].ToString()))
				.ForMember(dest => dest.IsAuthenticated, opt => opt.MapFrom(src => src.HttpContext.User.Identity.IsAuthenticated))
				.ForMember(dest => dest.IsLocal, opt => opt.MapFrom(src =>
					src.HttpContext.Connection.RemoteIpAddress.Equals(src.HttpContext.Connection.LocalIpAddress)));
        }
	}
}