using HibernatingRhinos.Loci.Common.Extensions;
using HibernatingRhinos.Loci.Common.Models;
using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Areas.Admin.ViewModels;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Helpers.Attributes;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.Services;
using RaccoonBlog.Web.ViewModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RaccoonBlog.Web.Areas.Admin.Models;
using Raven.Client.Documents.Commands.Batches;
using Sparrow.Json;

namespace RaccoonBlog.Web.Areas.Admin.Controllers
{
	public partial class PostsController : AdminController
	{
		private IAkismetService _akismetService;
		private readonly MediaService _mediaService;
        private readonly CacheSignalService _cacheSignal;
        public PostsController(IDocumentStore documentStore, IDocumentSession ravenSession, IAkismetService akismetService,  MediaService mediaService, CacheSignalService cacheSignal)
        : base(documentStore, ravenSession)
        {
            _akismetService = akismetService;
            _mediaService = mediaService;
			_cacheSignal = cacheSignal;
        }

        public virtual IActionResult Index()
		{
			// the actual UI is handled via JavaScript
			return View("List");
		}

		[HttpGet]
		public virtual IActionResult Add()
		{
			return View("Edit", new PostInput
			{
				AllowComments = true,
				ContentType = DynamicContentType.Html,
				CreatedAt = DateTimeOffset.Now,
				PublishAt = null // force auto schedule
			});
		}
		
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public IActionResult UploadImage(IFormFile file)
		{
			try
			{
				if (file == null || file.Length == 0)
					return BadRequest("No file uploaded.");

				using (var memoryStream = new MemoryStream())
				{
					file.CopyTo(memoryStream);
					
					var result = _mediaService.SaveImage(memoryStream, file.FileName, file.ContentType);
					
					var imageUrl = Url.Action("GetImage", "Images", new { area = "", id = result.FileHash, fileName = result.FileName });
            
					return Json(new { location = imageUrl });
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
			}
		}

		[HttpGet]
		public virtual IActionResult Edit(string id)
		{
			var post = RavenSession.Load<Post>("posts/" + id);
			if (post == null)
				return NotFound("Post does not exist.");
			return View(post.MapTo<PostInput>());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult Update(PostInput input)
		{
			if (!ModelState.IsValid)
				return View("Edit", input);

			var post = RavenSession.Load<Post>("posts/" + input.Id) ?? new Post {CreatedAt = DateTimeOffset.Now};
			input.MapPropertiesToInstance(post);

			// Be able to record the user making the actual post
			var user = RavenSession.GetCurrentUser(User);
			if (string.IsNullOrEmpty(post.AuthorId))
			{
				post.AuthorId = user.Id;
			}
			else
			{
				post.LastEditedByUserId = user.Id;
				post.LastEditedAt = DateTimeOffset.Now;
			}

			if (post.PublishAt == DateTimeOffset.MinValue)
			{
				var postScheduleringStrategy = new PostSchedulingStrategy(RavenSession, DateTimeOffset.Now);
				post.PublishAt = postScheduleringStrategy.Schedule();
			}

			// Actually save the post now
			RavenSession.Store(post);

			if (input.IsNewPost())
			{
				// Create the post comments object and link between it and the post
				var comments = new PostComments
				               {
				               	Comments = new List<PostComments.Comment>(),
				               	Spam = new List<PostComments.Comment>(),
				               	Post = new PostComments.PostReference
				               	       {
				               	       	Id = post.Id,
				               	       	PublishAt = post.PublishAt,
				               	       }
				               };

				RavenSession.Store(comments);
				post.CommentsId = comments.Id;	
			}

            _cacheSignal.Invalidate(CacheKeys.SectionArea);
            return RedirectToAction("Details", new {Id = post.MapTo<PostReference>().DomainId});
		}

		public virtual IActionResult Details(string id)
		{
			var post = RavenSession
				.Include<Post>(x => x.CommentsId)
				.Load("posts/" + id);

			if (post == null)
				return NotFound();

			var comments = RavenSession.Load<PostComments>(post.CommentsId);

			var vm = new AdminPostDetailsViewModel
			         {
			         	Post = post.MapTo<AdminPostDetailsViewModel.PostDetails>(),

			         	Comments = comments.Comments
			         		.Concat(comments.Spam)
			         		.OrderBy(comment => comment.CreatedAt)
			         		.MapTo<AdminPostDetailsViewModel.Comment>(),

			         	NextPost = RavenSession.GetNextPrevPost(post, true),
			         	PreviousPost = RavenSession.GetNextPrevPost(post, false),
			         	AreCommentsClosed = comments.AreCommentsClosed(post, BlogConfig.NumberOfDayToCloseComments),
			         };

			return View("Details", vm);
		}

		public virtual IActionResult ListFeed(DateTime start, DateTime end)
		{
			var posts = RavenSession.Query<Post>()
				.Where
				(
					post => post.PublishAt >= start &&
							post.PublishAt <= end
				)
				.OrderBy(post => post.PublishAt)
				.Take(256)
				.ToList();

			return Json(posts.MapTo<PostSummaryJson>());
		}


		[HttpPost]
		[AjaxOnly]
		[ValidateAntiForgeryToken]
		public virtual IActionResult SetPostDate(string id, long date)
		{
			var post = RavenSession
				.Include<Post>(x => x.CommentsId)
				.Load("posts/" + id);
			if (post == null)
				return Json(new {success = false});

			post.PublishAt = post.PublishAt.WithDate(DateTimeOffsetUtil.ConvertFromJsTimestamp(date));
			RavenSession.Load<PostComments>(post.CommentsId).Post.PublishAt = post.PublishAt;

			return Json(new {success = true});
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual IActionResult CommentsAdmin(string id, CommentCommandOptions command, int[] commentIds)
        {
			if (commentIds == null || commentIds.Length == 0)
				ModelState.AddModelError("CommentIdsAreEmpty", "Not comments was selected.");

			var post = RavenSession.Load<Post>("posts/" + id);
			if (post == null)
				return NotFound();

			if (ModelState.IsValid == false)
			{
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
					return Json(new {Success = false, message = ModelState.FirstErrorMessage()});

				return Details(id);
			}

			var comments = RavenSession.Load<PostComments>(post.CommentsId);
			switch (command)
			{
				case CommentCommandOptions.Delete:
					comments.Comments.RemoveAll(c => commentIds.Contains(c.Id));
					comments.Spam.RemoveAll(c => commentIds.Contains(c.Id));
					break;

				case CommentCommandOptions.MarkSpam:
					var spams = comments.Comments.Concat(comments.Spam)
						.Where(c => commentIds.Contains(c.Id))
						.ToArray();

					comments.Comments.RemoveAll(spams.Contains);
					comments.Spam.RemoveAll(spams.Contains);
					foreach (var comment in spams)
					{
                        _akismetService.MarkSpam(comment);
					}
					break;

				case CommentCommandOptions.MarkHam:
					var ham = comments.Spam
						.Where(c => commentIds.Contains(c.Id))
						.ToArray();

					comments.Spam.RemoveAll(ham.Contains);
					comments.Comments.AddRange(ham);

					comments.Comments
						.Where(c => c.IsSpam)
						.ForEach(comment =>
						         	{
						         		comment.IsSpam = false;
						         		_akismetService.MarkHam(comment);
						         		ResetNumberOfSpamComments(comment);
						         	});
					break;
				default:
					throw new InvalidOperationException(command + " command is not recognized.");
			}

			post.CommentsCount = comments.Comments.Count;

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return Json(new {Success = true});
			}
			return RedirectToAction("Details", new {id});
		}

		private void ResetNumberOfSpamComments(PostComments.Comment comment)
		{
			if (comment.CommenterId == null) 
				return;
			var commenter = RavenSession.Load<Commenter>(comment.CommenterId);
			if (commenter == null) 
				return;
			commenter.NumberOfSpamComments = 0;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual IActionResult Delete(string id)
		{
            var post = RavenSession.Load<Post>("posts/" + id);
		    if (post == null)
		        return SuccessResponse();

		    if (string.IsNullOrEmpty(post.CommentsId) == false)
            {
                RavenSession.Delete(post.CommentsId);
            }

            RavenSession.Delete(post);

            _cacheSignal.Invalidate(CacheKeys.SectionArea);
            return SuccessResponse();
        }

        private IActionResult SuccessResponse()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { Success = true });
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
		public virtual IActionResult DeleteAllSpamComments()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> DeleteAllSpamCommentsAsync(bool deleteAll)
		{
		    await DocumentStore.Operations.SendAsync(new PatchByQueryOperation(@"
from PostComments
where Spam.Count > 0
update {
    this.Spam = [];
}
"));
			return View();
		}

        [HttpGet]
        public virtual IActionResult AddIpToBlackList(string ipAddress)
        {
            var id = BlackList.GetId(ipAddress);
            var alreadyExists = RavenSession.Advanced.Exists(id);

            var viewModel = new AddIpToBlackListViewModel
            {
                IpAddress = ipAddress,
                AlreadySaved = alreadyExists
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual IActionResult AddIpToBlackList(AddIpToBlackListViewModel viewModel)
        {
            var ipAddress = viewModel.IpAddress;

            var id = BlackList.GetId(ipAddress);

            var blackList = RavenSession.Load<BlackList>(id);

            if (blackList == null)
            {
                blackList = BlackList.New(ipAddress);
                RavenSession.Store(blackList);
            }

            SetExpirationDate(blackList);

            RavenSession.SaveChanges();

            return SuccessResponse();
        }

        private void SetExpirationDate(BlackList blackList)
        {
            var expirationDate = DateTime.UtcNow.AddMonths(1);
            var metadata = RavenSession.Advanced.GetMetadataFor(blackList);

            metadata[Raven.Client.Constants.Documents.Metadata.Expires] = expirationDate;
        }
        

		public class PostBodyProjection
		{
			public string Id { get; set; }
			public string Body { get; set; }
		}

#if DEBUG
		[HttpGet("admin/posts/migrate-images")]
		[AllowAnonymous] 
		public IActionResult MigrateOldImages([FromServices] IWebHostEnvironment env)
		{
		    try
		    {
		        var archiveRootPath = env.WebRootPath;
		        
		        if (!Directory.Exists(archiveRootPath))
		            return Content($"Folder not found: {archiveRootPath}");

		        var urlMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		        int migratedCount = 0;
		        int updatedPostsCount = 0;
		        
		        var allFiles = Directory.GetFiles(archiveRootPath, "*.*", SearchOption.AllDirectories)
		                                .Where(f => 
		                                {
		                                    var ext = Path.GetExtension(f).ToLower();
		                                    return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif";
		                                }).ToList();
		        
		        int batchSize = 50;
		        for (int i = 0; i < allFiles.Count; i += batchSize)
		        {
		            var batchFiles = allFiles.Skip(i).Take(batchSize).ToList();
		            var openStreams = new List<FileStream>(); 

		            using (var fileSession = DocumentStore.OpenSession())
		            {
		                var batchData = batchFiles.Select(filePath => 
		                {
		                    var fileName = Path.GetFileName(filePath);
		                    string fileHash;
		                    string contentType = "image/" + Path.GetExtension(filePath).TrimStart('.').ToLower();
		                    if (contentType == "image/jpg") contentType = "image/jpeg";

		                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
		                    using (var sha1 = System.Security.Cryptography.SHA1.Create())
		                    {
		                        var hashBytes = sha1.ComputeHash(stream);
		                        fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
		                    }
		                    return new { Path = filePath, FileName = fileName, Hash = fileHash, ContentType = contentType, DocId = "images/" + fileHash };
		                }).ToList();
		                
		                var docIds = batchData.Select(x => x.DocId).Distinct().ToArray();
		                var existingDocs = fileSession.Load<PostImage>(docIds); 
		                
		                var processedIdsInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		                foreach (var data in batchData)
		                {
		                    if (existingDocs[data.DocId] == null && !processedIdsInBatch.Contains(data.DocId))
		                    {
		                        var imageDoc = new PostImage { Id = data.DocId, UploadedAt = DateTimeOffset.Now, FileName = data.FileName };
		                        fileSession.Store(imageDoc);
		                        
		                        var attachmentStream = new FileStream(data.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
		                        fileSession.Advanced.Attachments.Store(data.DocId, data.FileName, attachmentStream, data.ContentType);
		                        openStreams.Add(attachmentStream); 

		                        processedIdsInBatch.Add(data.DocId);
		                        migratedCount++;
		                    }
		                    
		                    var newUrl = Url.Action("GetImage", "Images", new { area = "", id = data.Hash, fileName = data.FileName });

		                    var relativePath = data.Path.Substring(archiveRootPath.Length).Replace("\\", "/").TrimStart('/').ToLower();
		                    var parts = relativePath.Split('/');
		                    
		                    if (parts.Length >= 2)
		                    {
		                        var folderAndFileKey = parts[parts.Length - 2] + "/" + parts[parts.Length - 1];
		                        urlMap[folderAndFileKey] = newUrl;
		                    }

		                    if (!data.FileName.StartsWith("image", StringComparison.OrdinalIgnoreCase) && 
		                        !data.FileName.StartsWith("wlEmoticon", StringComparison.OrdinalIgnoreCase) &&
		                        !data.FileName.StartsWith("clip_image", StringComparison.OrdinalIgnoreCase))
		                    {
		                        if (!urlMap.ContainsKey(data.FileName)) 
		                            urlMap[data.FileName] = newUrl;
		                    }
		                }
		                
		                fileSession.SaveChanges(); 
		                foreach (var s in openStreams) s.Dispose(); 
		            }
		        }
		        
		        var oldUrlRegex = new Regex(@"(?:https?://(?:www\.)?ayende\.com)?/(?:blog/)?(?:Content|Images|Blog/Images|Open-Live-Writer|Windows-Live-Writer|WindowsLiveWriter|ayende_com)[^"">]+?\.(?:png|jpg|jpeg|gif)", 
		            RegexOptions.Compiled | RegexOptions.IgnoreCase);

		        var query = RavenSession.Query<Post>()
		            .Select(p => new PostBodyProjection { Id = p.Id, Body = p.Body });

		        using (var stream = RavenSession.Advanced.Stream(query))
		        using (var bulkSession = DocumentStore.OpenSession()) 
		        {
		            int batchChangesCount = 0;
		            
		            while (stream.MoveNext())
		            {
		                var doc = stream.Current.Document; 
		                if (string.IsNullOrEmpty(doc.Body)) continue;

		                bool isModified = false;
		                string newBody = doc.Body;

		                newBody = oldUrlRegex.Replace(doc.Body, match =>
		                {
		                    var fullMatch = match.Value;
		                    string decodedMatch;
		                    
		                    try { decodedMatch = Uri.UnescapeDataString(fullMatch).ToLower(); }
		                    catch { decodedMatch = fullMatch.ToLower(); }
		                    
		                    var urlParts = decodedMatch.TrimEnd('/').Split('/');
		                    var fileNameInUrl = urlParts.Last();
		                    
		                    if (urlParts.Length >= 2)
		                    {
		                        var folderAndFileKey = urlParts[urlParts.Length - 2] + "/" + urlParts[urlParts.Length - 1];
		                        if (urlMap.TryGetValue(folderAndFileKey, out string newUrl1))
		                        {
		                            isModified = true;
		                            return newUrl1;
		                        }
		                    }
		                    
		                    if (urlMap.TryGetValue(fileNameInUrl, out string newUrl2))
		                    {
		                        isModified = true;
		                        return newUrl2;
		                    }

		                    return fullMatch;
		                });

		                if (isModified)
		                {
		                    bulkSession.Advanced.Patch<Post, string>(doc.Id, p => p.Body, newBody);
		                    updatedPostsCount++;
		                    batchChangesCount++;
		                }

		                if (batchChangesCount >= 500)
		                {
		                    bulkSession.SaveChanges();
		                    bulkSession.Advanced.Clear();
		                    batchChangesCount = 0;
		                }
		            }
		            
		            if (batchChangesCount > 0)
		            {
		                bulkSession.SaveChanges();
		            }
		        }

		        return Content($@"
		            <h1>Migration complete!</h1>
		            <p>Images added to RavenDB: {migratedCount} (of {allFiles.Count} found on disk)</p>
		            <p>Posts updated: {updatedPostsCount}</p>
		            <p>Total number of unique paths in the dictionary: {urlMap.Count}</p>
		        ", "text/html; charset=utf-8");
		    }
		    catch (Exception ex)
		    {
		        return Content($"<h1>Error:</h1><pre>{ex.Message}\n{ex.StackTrace}</pre>", "text/html; charset=utf-8");
		    }
		}
#endif
	}

	public enum CommentCommandOptions
	{
		Delete,
		MarkHam,
		MarkSpam
	}
}