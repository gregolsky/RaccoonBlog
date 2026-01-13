using System;
using Microsoft.AspNetCore.Html;
using AutoMapper;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;

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
	    }
	}
}