using AutoMapper;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;
using Microsoft.AspNetCore.Html;

namespace RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles
{
	public class EmailViewModelMapperProfile : Profile
	{
	    public EmailViewModelMapperProfile()
	    {
			CreateMap<PostComments.Comment, NewCommentEmailViewModel>()
				.ForMember(x=>x.Body, o=>o.MapFrom(x=> new HtmlString(MarkdownResolver.Resolve(x.Body))))
				.ForMember(x => x.PostId, o => o.Ignore())
				.ForMember(x => x.PostTitle, o => o.Ignore())
				.ForMember(x => x.PostSlug, o => o.Ignore())
				.ForMember(x => x.BlogName, o => o.Ignore())
				.ForMember(x => x.Key, o => o.Ignore())
				.ForMember(x => x.IpAddress, o => o.MapFrom(x => x.UserHostAddress))
				.ForMember(x => x.UserAgent, o => o.MapFrom(x => x.UserAgent))
				;

			// Note: Commenter -> CommentInput and User -> CommentInput mappings 
			// are in PostViewModelMapperProfile to avoid duplication
		}
	}
}
