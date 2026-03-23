using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Infrastructure.Indexes;
using RaccoonBlog.Web.Models;
using Raven.Client.Documents;
using WilderMinds.MetaWeblog;
using Post = WilderMinds.MetaWeblog.Post;

namespace RaccoonBlog.Web.Services;

public class MetaWeblogService : IMetaWeblogProvider
{
    private readonly IDocumentStore documentStore;
    private readonly IConfiguration configuration;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly MediaService _mediaService;

    public MetaWeblogService(
        IDocumentStore documentStore,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        MediaService mediaService)
    {
        this.documentStore = documentStore;
        this.configuration = configuration;
        this.httpContextAccessor = httpContextAccessor;
        _mediaService = mediaService;
    }
    
    private IUrlHelper Url => new UrlHelper(httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ActionContext>());

    public Task<string> AddPostAsync(string blogid, string username, string password, Post post, bool publish)
    {
        Models.Post newPost;
        using (var session = documentStore.OpenSession())
        {
            var user = ValidateUser(username, password);
            var comments = new PostComments
            {
                Comments = new List<PostComments.Comment>(),
                Spam = new List<PostComments.Comment>()
            };
            session.Store(comments);

            var postScheduleringStrategy = new PostSchedulingStrategy(session, DateTimeOffset.Now);
            var publishDate = post.dateCreated == DateTime.MinValue
                ? postScheduleringStrategy.Schedule()
                : postScheduleringStrategy.Schedule(new DateTimeOffset(post.dateCreated));

            newPost = new Models.Post
            {
                AuthorId = user.Id,
                Body = post.description,
                CommentsId = comments.Id,
                CreatedAt = DateTimeOffset.Now,
                SkipAutoReschedule = post.dateCreated != DateTime.MinValue,
                PublishAt = publishDate,
                Tags = post.categories.ToList(),
                Title = post.title,
                CommentsCount = 0,
                AllowComments = true,
            };
            session.Store(newPost);
            comments.Post = new PostComments.PostReference
            {
                Id = newPost.Id,
                PublishAt = publishDate
            };

            session.SaveChanges();
        }
        return Task.FromResult(newPost.Id);
    }

    public Task<bool> EditPostAsync(string postid, string username, string password, Post post, bool publish)
    {
        using (var session = documentStore.OpenSession())
        {
            var user = ValidateUser(username, password);
            var postToEdit = session
                .Include<Models.Post>(x => x.CommentsId)
                .Load<Models.Post>(postid);
            
            if (postToEdit == null)
                throw new MetaWeblogException("Post does not exists");

            if (string.IsNullOrEmpty(postToEdit.AuthorId))
                postToEdit.AuthorId = user.Id;
            else
            {
                postToEdit.LastEditedByUserId = user.Id;
                postToEdit.LastEditedAt = DateTimeOffset.Now;
            }

            postToEdit.Body = post.description;
            if (postToEdit.SkipAutoReschedule && post.dateCreated != DateTime.MinValue && post.dateCreated != postToEdit.PublishAt.DateTime)
            {
                var postScheduleringStrategy = new PostSchedulingStrategy(session, DateTimeOffset.Now);
                postToEdit.PublishAt = postScheduleringStrategy.Schedule(new DateTimeOffset(post.dateCreated));
                session.Load<PostComments>(postToEdit.CommentsId).Post.PublishAt = postToEdit.PublishAt;
            }
            postToEdit.Tags = post.categories.ToList();
            postToEdit.Title = post.title;

            session.SaveChanges();
        }
        return Task.FromResult(true);
    }

    public Task<Post> GetPostAsync(string postid, string username, string password)
    {
        ValidateUser(username, password);
        using (var session = documentStore.OpenSession())
        {
            var thePost = session.Load<Models.Post>(postid);
            if (thePost == null)
                throw new InvalidOperationException("You cannot get deleted post");

            return Task.FromResult(new Post
            {
                wp_slug = SlugConverter.TitleToSlug(thePost.Title),
                description = thePost.Body,
                dateCreated = thePost.PublishAt.DateTime,
                categories = thePost.Tags.ToArray(),
                title = thePost.Title,
                postid = thePost.Id,
            });
        }
    }

    public Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int numberOfPosts)
    {
        ValidateUser(username, password);
        using (var session = documentStore.OpenSession())
        {
            var list = session.Query<Models.Post>()
                .OrderByDescending(x => x.PublishAt)
                .Take(numberOfPosts)
                .ToList();

            return Task.FromResult(list.Select(thePost => new Post
            {
                wp_slug = SlugConverter.TitleToSlug(thePost.Title),
                description = thePost.Body,
                dateCreated = thePost.PublishAt.DateTime,
                categories = thePost.Tags.ToArray(),
                title = thePost.Title,
                postid = thePost.Id,
            }).ToArray());
        }
    }

    public Task<CategoryInfo[]> GetCategoriesAsync(string blogid, string username, string password)
    {
        ValidateUser(username, password);
        var mostRecentTag = new DateTimeOffset(DateTimeOffset.Now.Year - 2, DateTimeOffset.Now.Month, 1, 0, 0, 0, DateTimeOffset.Now.Offset);
        using (var session = documentStore.OpenSession())
        {
            var categoryInfos = session.Query<Tags_Count.ReduceResult, Tags_Count>()
                .Where(x => x.LastSeenAt > mostRecentTag)
                .ToList();

            return Task.FromResult(categoryInfos.Select(x => new CategoryInfo
            {
                categoryid = x.Name,
                description = x.Name,
                title = x.Name,
                htmlUrl = $"/blog/tag/{x.Name}",
                rssUrl = $"/blog/tag/{x.Name}/rss"
            }).ToArray());
        }
    }

    public Task<MediaObjectInfo> NewMediaObjectAsync(string blogid, string username, string password, MediaObject mediaObject)
    {
        ValidateUser(username, password);
        byte[] imageBytes = Convert.FromBase64String(mediaObject.bits);
        using (var memoryStream = new MemoryStream(imageBytes))
        {
            var savedFileName = _mediaService.SaveImage(memoryStream, mediaObject.name, mediaObject.type ?? "application/octet-stream");
            var imageUrl = $"/blog/Images/{savedFileName}";
            return Task.FromResult(new MediaObjectInfo { url = imageUrl });
        }
    }

    public Task<bool> DeletePostAsync(string key, string postid, string username, string password, bool publish)
    {
        ValidateUser(username, password);
        using (var session = documentStore.OpenSession())
        {
            var thePost = session.Load<Models.Post>(postid);
            if (thePost != null)
            {
                if (!string.IsNullOrEmpty(thePost.CommentsId)) session.Delete(thePost.CommentsId);
                session.Delete(thePost);
            }
            session.SaveChanges();
        }
        return Task.FromResult(true);
    }

    public Task<BlogInfo[]> GetUsersBlogsAsync(string key, string username, string password)
    {
        ValidateUser(username, password);
        var request = httpContextAccessor.HttpContext.Request;
        return Task.FromResult(new[] {
            new BlogInfo {
                blogid = "blogs/1",
                blogName = username,
                url = $"{request.Scheme}://{request.Host}{request.PathBase}"
            }
        });
    }

    public Task<UserInfo> GetUserInfoAsync(string key, string username, string password)
    {
        var user = ValidateUser(username, password);
        return Task.FromResult(new UserInfo
        {
            email = user.Email,
            nickname = user.FullName,
            firstname = user.FullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(),
            lastname = user.FullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault(),
            userid = user.Id
        });
    }

    public Task<int> AddCategoryAsync(string key, string username, string password, NewCategory category)
    {
        ValidateUser(username, password);
        return Task.FromResult(1); 
    }

    private User ValidateUser(string username, string password)
    {
        User user;
        using (var session = documentStore.OpenSession())
        {
            user = session.GetUserByEmail(username);
        }
        if (user == null || user.ValidatePassword(password) == false)
            throw new MetaWeblogException("User is not valid!");
        if (user.Enabled == false)
            throw new MetaWeblogException("User is not enabled!");
        return user;
    }
    
    public Task<Tag[]> GetTagsAsync(string blogid, string username, string password) => throw new NotImplementedException();
    public Task<Page> GetPageAsync(string blogid, string pageid, string username, string password) => throw new NotImplementedException();
    public Task<Page[]> GetPagesAsync(string blogid, string username, string password, int numPages) => throw new NotImplementedException();
    public Task<Author[]> GetAuthorsAsync(string blogid, string username, string password) => throw new NotImplementedException();
    public Task<string> AddPageAsync(string blogid, string username, string password, Page page, bool publish) => throw new NotImplementedException();
    public Task<bool> EditPageAsync(string blogid, string pageid, string username, string password, Page page, bool publish) => throw new NotImplementedException();
    public Task<bool> DeletePageAsync(string blogid, string username, string password, string pageid) => throw new NotImplementedException();
}