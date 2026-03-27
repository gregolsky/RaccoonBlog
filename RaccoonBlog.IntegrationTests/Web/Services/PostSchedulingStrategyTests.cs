using System;
using System.Linq;
using RaccoonBlog.Web.Infrastructure.Common;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Embedded;
using Xunit;

namespace RaccoonBlog.IntegrationTests.Web.Services
{
	public class PostSchedulingStrategyTests : IDisposable
	{
	    private static EmbeddedServer _embeddedServer;
	    private static readonly object _lock = new object();
	    
		protected DateTimeOffset Now { get; private set; }
		protected IDocumentStore DocumentStore { get; private set; }
		protected IDocumentSession Session { get; private set; }

		public PostSchedulingStrategyTests()
		{
			lock (_lock)
			{
				if (_embeddedServer == null)
				{
					var server = EmbeddedServer.Instance;
					server.StartServer();
					_embeddedServer = server;
				}
			}
			
			Now = DateTimeOffset.Now;
		    DocumentStore = _embeddedServer.GetDocumentStore(Guid.NewGuid().ToString());
			Session = DocumentStore.OpenSession();
		}

		public virtual void Dispose()
		{
			Session?.Dispose();
			DocumentStore?.Dispose();
		}

		[Fact]
		public void WhenPostingNewPostWithoutPublishDateSpecified_AndTheLastPostPublishDateIsAFewDaysAgo_ScheduleItForTomorrowAtNoot()
		{
			Session.Store(new Post { PublishAt = Now.AddDays(-3) });
			Session.SaveChanges();

			var rescheduler = new PostSchedulingStrategy(Session, Now);

			var result = rescheduler.Schedule();
			Assert.Equal(Now.AddDays(1).SkipToNextWorkDay().AtNoon(), result);
		}

		[Fact]
		public void WhenPostingNewPostWithoutPublishDateSpecified_AndThereIsNoLastPost_ScheduleItForTomorrowAtNoot()
		{
			var rescheduler = new PostSchedulingStrategy(Session, Now);

			var result = rescheduler.Schedule();
			Assert.Equal(Now.AddDays(1).SkipToNextWorkDay().AtNoon(), result);
		}

		[Fact]
		public void WhenPostingNewPostWithPublishDateSpecified_AndTheLastPostPublishDateIsAFewDaysAgo_ScheduleItForSpecifiedDate()
		{
			Session.Store(new Post { PublishAt = Now.AddDays(-3) });
			Session.SaveChanges();

			var rescheduler = new PostSchedulingStrategy(Session, Now);

			var scheduleDate = Now.AddHours(1);
			var result = rescheduler.Schedule(scheduleDate);
			Assert.Equal(scheduleDate, result);
			Assert.NotEqual(scheduleDate.AddDays(-2), result);
		}

		[Fact]
		public void WhenPostingNewPostWithPublishDateSpecified_AndThereIsNoLastPost_ScheduleItForSpecifiedDate()
		{
			var rescheduler = new PostSchedulingStrategy(Session, Now);

			var scheduleDate = Now.AddHours(1);
			var result = rescheduler.Schedule(scheduleDate);
			Assert.Equal(scheduleDate, result);
		}

		[Fact]
		public void WhenPostingNewPost_DoNotReschedulePublishedPosts()
		{
			Session.Store(new Post { PublishAt = Now.AddDays(-3) });
			Session.Store(new Post { PublishAt = Now.AddHours(-1) });
			Session.SaveChanges();

			var rescheduler = new PostSchedulingStrategy(Session, Now);
			rescheduler.Schedule();

			Assert.Empty(Session.Query<Post>().Where(post => post.PublishAt > Now));
		}

		[Fact]
		public void WhenPostingNewPostWithPublishDateSpecified_BeforePostsAlreadyPublished_DoNotReschedulePublishedPosts()
		{
			Session.Store(new Post { PublishAt = Now.AddDays(-3) });
			Session.Store(new Post { PublishAt = Now.AddHours(-1) });
			Session.SaveChanges();

			var rescheduler = new PostSchedulingStrategy(Session, Now);
			rescheduler.Schedule(Now.AddHours(-2));

			Assert.Empty(Session.Query<Post>().Where(post => post.PublishAt > Now));
		}

		[Fact]
		public void UpdatePublishDateOfExistintPost_WillUpdateTheDateAndTimeCorrectly()
		{
			Session.Store(new Post { PublishAt = Now.AddDays(-3) });
			Session.Store(new Post { PublishAt = Now.AddHours(-1) });
			Session.Store(new Post { PublishAt = Now.AddHours(12) });
			Session.SaveChanges();

			var lastPost = Session.Query<Post>()
				.OrderByDescending(post => post.PublishAt)
				.First();
			Assert.Equal(Now.AddHours(12), lastPost.PublishAt);

			var rescheduler = new PostSchedulingStrategy(Session, Now);
			lastPost.PublishAt = rescheduler.Schedule(Now.AddHours(6));
			Session.SaveChanges();

			Assert.Equal(Now.AddHours(6), lastPost.PublishAt);
		}
	}
}