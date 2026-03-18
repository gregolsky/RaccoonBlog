using Microsoft.AspNetCore.Html;
using AutoMapper;

namespace RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers
{
	public class MvcHtmlStringConverter : ITypeConverter<string, IHtmlContent>
	{
	    public IHtmlContent Convert(string source, IHtmlContent destination, ResolutionContext context)
	    {
	        return new HtmlString(source ?? string.Empty);
	    }
	}
}