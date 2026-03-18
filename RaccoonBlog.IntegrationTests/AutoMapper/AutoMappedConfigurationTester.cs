using AutoMapper;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;
using Xunit;

namespace RaccoonBlog.IntegrationTests.AutoMapper
{
	public class AutoMapperConfigurationTester
	{
		private readonly IMapper _mapper;
		private readonly MapperConfiguration _configuration;

		public AutoMapperConfigurationTester()
		{
			_configuration = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<AutoMapperConfiguration>();
				cfg.AddProfile<PostViewModelMapperProfile>();
				cfg.AddProfile<PostsViewModelMapperProfile>();
				cfg.AddProfile<TagsListViewModelMapperProfile>();
				cfg.AddProfile<SectionMapperProfile>();
				cfg.AddProfile<EmailViewModelMapperProfile>();
				cfg.AddProfile<SeriesMapperProfile>();
				cfg.AddProfile<UserAdminMapperProfile>();
				cfg.AddProfile<PostsAdminViewModelMapperProfile>();
			});

			_mapper = _configuration.CreateMapper();

			AutoMapperExtensions.Initialize(_mapper);
		}

		[Fact]
		public void AssertConfigurationIsValid()
		{
			_configuration.AssertConfigurationIsValid();
		}

		[Fact]
		public void CanMapFromCommentToNewCommentViewModel()
		{
			var comment = new PostComments.Comment();
			var ex = Record.Exception(() => comment.MapTo<NewCommentEmailViewModel>());
			Assert.Null(ex);
		}
	}
}