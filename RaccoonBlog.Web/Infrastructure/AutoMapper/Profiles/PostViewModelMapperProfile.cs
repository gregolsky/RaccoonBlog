using System.Configuration;
using System.Linq;
using System.Web;
using AutoMapper;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles
{
	public class PostViewModelMapperProfile : AbstractProfile
	{
		protected override void Configure()
		{
			Mapper.CreateMap<Post, PostViewModel.PostDetails>()
				.ForMember(x => x.Id, o => o.MapFrom(m => RavenIdResolver.Resolve(m.Id)))
				.ForMember(x => x.Slug, o => o.MapFrom(m => SlugConverter.TitleToSlug(m.Title)))
				.ForMember(x => x.PublishedAt, o => o.MapFrom(m => m.PublishAt))
				.ForMember(x => x.IsCommentAllowed, o => o.MapFrom(m => m.AllowComments))
				.ForMember(x => x.Title, o => o.MapFrom(m => HttpUtility.HtmlDecode(m.Title)))
				.ForMember(x => x.Author, o => o.Ignore())
				;

			Mapper.CreateMap<PostComments.Comment, PostViewModel.Comment>()
                .ForMember(x => x.CommenterKey, o => o.MapFrom(m => m.CommenterKey))
				.ForMember(x => x.Body, o => o.MapFrom(m => MarkdownResolver.Resolve(m.Body)))
				.ForMember(x => x.EmailHash, o => o.MapFrom(m => EmailHashResolver.Resolve(m.Email)))
				.ForMember(x => x.IsImportant, o => o.MapFrom(m => m.Important))
				.ForMember(x => x.Url, o => o.MapFrom(m => UrlResolver.Resolve(m.Url)))
				.ForMember(x => x.Tooltip, o => o.MapFrom(m => string.IsNullOrEmpty(m.Url) ? "Comment by " + m.Author : m.Url))
				.ForMember(x => x.CreatedAt, o => o.MapFrom(m => m.CreatedAt.ToUniversalTime().ToString("MM/dd/yyyy hh:mm tt")))
				;

			Mapper.CreateMap<Post, PostReference>()
				.ForMember(x => x.Title, o => o.MapFrom(m => HttpUtility.HtmlDecode(m.Title)))
				.ForMember(x => x.Slug, o => o.Ignore())
				;
			
			Mapper.CreateMap<Commenter, CommentInput>()
				.ForMember(x => x.Body, o => o.Ignore())
				.ForMember(x => x.CommenterKey, o => o.MapFrom(m => m.Key))
				;

			Mapper.CreateMap<CommentInput, Commenter>()
				.ForMember(x => x.Id, o => o.Ignore())
				.ForMember(x => x.IsTrustedCommenter, o => o.Ignore())
				.ForMember(x => x.Key, o => o.Ignore())
				.ForMember(x => x.OpenId, o => o.Ignore())
				.ForMember(x => x.NumberOfSpamComments, o => o.Ignore())
				;

			Mapper.CreateMap<User, CommentInput>()
				.ForMember(x => x.Name, o => o.MapFrom(m => m.FullName))
				.ForMember(x => x.Url, o => o.MapFrom(m => UrlHelper.RelativeToAbsolute(UrlHelper.RouteUrl("homepage"))))
				.ForMember(x => x.Body, o => o.Ignore())
				.ForMember(x => x.CommenterKey, o => o.Ignore())
				;

			//Mapper.CreateMap<UserProfile, CommentInput>()
			//    .ForMember(x => x.Name, o => o.MapFrom(m => m.FirstName + " " + m.LastName))
			//    .ForMember(x => x.Url, o => o.MapFrom(m => m.ProfileURL))
			//    .ForMember(x => x.Body, o => o.Ignore())
			//    .ForMember(x => x.CommenterKey, o => o.Ignore())
			//    ;

			Mapper.CreateMap<HttpRequestWrapper, Tasks.AddCommentTask.RequestValues>();

			Mapper.CreateMap<User, PostViewModel.UserDetails>();
		}
	}
}
