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
	        // Global type converters
	        CreateMap<string, IHtmlContent>().ConvertUsing<MvcHtmlStringConverter>();
	        CreateMap<Guid, string>().ConvertUsing<GuidToStringConverter>();
	        CreateMap<DateTimeOffset, DateTime>().ConvertUsing<DateTimeTypeConverter>();
	    }
	}
}