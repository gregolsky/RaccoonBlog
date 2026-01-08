using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using HibernatingRhinos.Loci.Common.Extensions;
using HibernatingRhinos.Loci.Common.Tasks;
using RaccoonBlog.Web.Models;

namespace RaccoonBlog.Web.Infrastructure.Tasks
{
	public class SendEmailTask : BackgroundTask
	{
		private readonly string replyTo;
		private readonly string subject;
		private readonly string view;
		private readonly object model;
		private readonly string sendTo;
		private readonly IServiceProvider serviceProvider;

		public SendEmailTask(
			string replyTo,
			string subject,
			string view,
			string sendTo,
			object model,
			IServiceProvider serviceProvider)
		{
			this.replyTo = replyTo;
			this.subject = subject;
			this.view = view;
			this.model = model;
			this.sendTo = sendTo;
			this.serviceProvider = serviceProvider;
		}

		static SendEmailTask()
		{
			// Fix: The remote certificate is invalid according to the validation procedure.
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
		}

		public override void Execute()
		{
			ExecuteAsync().GetAwaiter().GetResult();
		}

		private async Task ExecuteAsync()
		{
			var razorViewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
			var tempDataProvider = serviceProvider.GetRequiredService<ITempDataProvider>();
			var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

			var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

			var viewResult = razorViewEngine.FindView(actionContext, view, false);
			if (!viewResult.Success)
			{
				throw new InvalidOperationException($"Could not find view: {view}");
			}

			var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
			{
				Model = model
			};

			using (var stringWriter = new StringWriter())
			{
				var viewContext = new ViewContext(
					actionContext,
					viewResult.View,
					viewDictionary,
					new TempDataDictionary(httpContext, tempDataProvider),
					stringWriter,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);

				var mailMessage = new MailMessage
				{
					IsBodyHtml = true,
					Body = stringWriter.ToString(),
					Subject = subject,
				};

				if (string.IsNullOrEmpty(replyTo) == false)
				{
					try
					{
						mailMessage.ReplyToList.Add(new MailAddress(replyTo));
					}
					catch
					{
						// we explicitly ignore bad reply to emails
					}
				}

				// Send a notification of the comment to the post author
				mailMessage.To.Add(sendTo);

				// Also CC the owners, if specified
				OwnerEmails.ForEach(email => mailMessage.CC.Add(email));

				using (var smtpClient = new SmtpClient())
				{
					await smtpClient.SendMailAsync(mailMessage);
				}
			}
		}

		public IEnumerable<MailAddress> OwnerEmails
		{
			get
			{
				var commentsMederatorEmails = DocumentSession.Load<BlogConfig>(BlogConfig.Key).OwnerEmail;
				return commentsMederatorEmails
					.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => new MailAddress(x.Trim()));
			}
		}
	}
}