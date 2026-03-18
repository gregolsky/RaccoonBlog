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
using Microsoft.Extensions.Configuration;
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
        private readonly IServiceScopeFactory scopeFactory;

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
            this.scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
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
            using (var scope = scopeFactory.CreateScope())
            {
                var sp = scope.ServiceProvider;

                var razorViewEngine = sp.GetRequiredService<IRazorViewEngine>();
                var tempDataProvider = sp.GetRequiredService<ITempDataProvider>();
                var configuration = sp.GetRequiredService<IConfiguration>();

                var mainUrlString = configuration["AppSettings:MainUrl"] ?? "https://localhost";
                var mainUri = new Uri(mainUrlString);

                var httpContext = new DefaultHttpContext { RequestServices = sp };
                httpContext.Request.Scheme = mainUri.Scheme;
                httpContext.Request.Host = HostString.FromUriComponent(mainUri);

                if (!string.IsNullOrEmpty(mainUri.AbsolutePath) && mainUri.AbsolutePath != "/")
                {
                    httpContext.Request.PathBase = mainUri.AbsolutePath;
                }

                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                if (httpContextAccessor != null)
                {
                    httpContextAccessor.HttpContext = httpContext;
                }

                var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

                var viewPath = $"~/Views/MailTemplates/{view}.cshtml";
                var viewResult = razorViewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: true);
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

                    var smtpHost = configuration["SmtpSettings:Host"];
                    var smtpPort = configuration.GetValue<int>("SmtpSettings:Port", 587);
                    var smtpUser = configuration["SmtpSettings:UserName"];
                    var smtpPass = configuration["SmtpSettings:Password"];
                    var enableSsl = configuration.GetValue<bool>("SmtpSettings:EnableSsl", true);
                    var fromEmail = configuration["SmtpSettings:From"];

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
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

                    mailMessage.To.Add(sendTo);

                    var ownerEmails = OwnerEmails.ToList();
                    ownerEmails.ForEach(email => mailMessage.CC.Add(email));

                    using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                    {
                        if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPass))
                        {
                            smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);
                            smtpClient.EnableSsl = enableSsl;
                        }

                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }
            }
        }

        public IEnumerable<MailAddress> OwnerEmails
        {
            get
            {
                var blogConfig = DocumentSession.Load<BlogConfig>(BlogConfig.Key);
                var commentsMederatorEmails = blogConfig?.OwnerEmail;

                if (string.IsNullOrWhiteSpace(commentsMederatorEmails))
                    return Enumerable.Empty<MailAddress>();

                return commentsMederatorEmails
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new MailAddress(x.Trim()));
            }
        }
    }
}