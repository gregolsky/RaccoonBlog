using AutoMapper;
using FluentScheduler;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using NLog.Web;
using RaccoonBlog.Web.Helpers;
using RaccoonBlog.Web.Helpers.Binders;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Indexes;
using RaccoonBlog.Web.Infrastructure.Jobs;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.Client.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Initialize ConfigurationHelper early
ConfigurationHelper.Initialize(builder.Configuration);

// Configure NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new GuidBinderProvider());
})
.AddNewtonsoftJson();

// Configure Session (required for session-based TempData)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure TempData to use JSON serialization instead of BSON
builder.Services.AddSingleton<TempDataSerializer, JsonTempDataSerializer>();

builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<RaccoonBlog.Web.Helpers.SignInHelper>();
// Configure RavenDB DocumentStore
var ravenUrls = builder.Configuration["Raven:Urls"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "http://localhost:8080" };
var ravenDatabase = builder.Configuration["Raven:Database"] ?? "blog.ayende.com";

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
ServicePointManager.CheckCertificateRevocationList = false;
ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

var documentStore = new DocumentStore
{
    Urls = ravenUrls,
    Database = ravenDatabase,
    Conventions = new DocumentConventions
    {
        AggressiveCache = { Mode = AggressiveCacheMode.DoNotTrackChanges }
    }
};

// Certificate configuration
var certificatePath = builder.Configuration["Raven:CertificatePath"];
if (!string.IsNullOrEmpty(certificatePath))
{
    var certificatePassword = builder.Configuration["Raven:CertificatePassword"];
    documentStore.Certificate = new X509Certificate2(certificatePath, certificatePassword);
}

// Request timeout configuration
if (int.TryParse(builder.Configuration["Raven:RequestsTimeoutInSec"], out int timeoutSeconds))
{
    documentStore.Conventions.RequestTimeout = TimeSpan.FromSeconds(timeoutSeconds);
}

documentStore.Initialize();
builder.Services.AddSingleton<IDocumentStore>(documentStore);
builder.Services.AddScoped<IDocumentSession>(ctx =>
{
    return ctx.GetRequiredService<IDocumentStore>().OpenSession();
});

builder.Services.AddScoped<RaccoonBlog.Web.Models.BlogConfig>(ctx =>
{
    var session = ctx.GetRequiredService<IDocumentSession>();
    using (session.Advanced.DocumentStore.AggressivelyCacheFor(TimeSpan.FromMinutes(5)))
    {
        return session.Load<RaccoonBlog.Web.Models.BlogConfig>("Blog/Config")
               ?? new RaccoonBlog.Web.Models.BlogConfig();
    }
});

// Configure Authentication
var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
    });

// Only add OAuth providers if credentials are configured
var googleClientId = builder.Configuration["Raccoon:OAuth:Google:ClientId"];
var googleClientSecret = builder.Configuration["Raccoon:OAuth:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

var microsoftClientId = builder.Configuration["Raccoon:OAuth:Microsoft:ClientId"];
var microsoftClientSecret = builder.Configuration["Raccoon:OAuth:Microsoft:ClientSecret"];
if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret))
{
    authBuilder.AddMicrosoftAccount(options =>
    {
        options.ClientId = microsoftClientId;
        options.ClientSecret = microsoftClientSecret;
    });
}

var facebookAppId = builder.Configuration["Raccoon:OAuth:Facebook:AppId"];
var facebookAppSecret = builder.Configuration["Raccoon:OAuth:Facebook:AppSecret"];
if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = facebookAppId;
        options.AppSecret = facebookAppSecret;
    });
}

var twitterConsumerKey = builder.Configuration["Raccoon:OAuth:Twitter:ConsumerKey"];
var twitterConsumerSecret = builder.Configuration["Raccoon:OAuth:Twitter:ConsumerSecret"];
if (!string.IsNullOrEmpty(twitterConsumerKey) && !string.IsNullOrEmpty(twitterConsumerSecret))
{
    authBuilder.AddTwitter(options =>
    {
        options.ConsumerKey = twitterConsumerKey;
        options.ConsumerSecret = twitterConsumerSecret;
    });
}

// Configure AutoMapper using modern DI pattern for AutoMapper 12.x
// This automatically registers IMapper in DI and scans for profiles
builder.Services.AddAutoMapper(cfg =>
{
    // Add only specific profiles to avoid scanning classes that reference System.Web
    // Don't use assembly scanning as it will scan ALL types including MetaWeblog which has System.Web dependencies
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.AutoMapperConfiguration>();
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.PostViewModelMapperProfile>();
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.PostsViewModelMapperProfile>();
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.TagsListViewModelMapperProfile>();
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.SectionMapperProfile>();
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.EmailViewModelMapperProfile>();
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.SeriesMapperProfile>();
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.UserAdminMapperProfile>();
    cfg.AddProfile<RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.PostsAdminViewModelMapperProfile>();
});

// Initialize FluentScheduler jobs
JobManager.JobException += info =>
{
    var logger = LogManager.GetCurrentClassLogger();
    logger.Fatal(info.Exception, $"Error executing background job {info.Name}.");
};
JobManager.Initialize(new SocialNetworkIntegrationJobsRegistry());

var app = builder.Build();

// Initialize AutoMapper extensions with the IMapper instance from the ROOT service provider
// IMPORTANT: Do NOT use a scoped service provider here, as it will be disposed
// and AutoMapper will try to use the disposed provider for type converters
var mapper = app.Services.GetRequiredService<AutoMapper.IMapper>();
RaccoonBlog.Web.Infrastructure.AutoMapper.AutoMapperExtensions.Initialize(mapper);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/Index");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware (must be before authentication and authorization)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// RavenDB session management per request
//app.Use(async (context, next) =>
//{
//    var session = documentStore.OpenSession();
//    context.Items["CurrentRequestRavenSession"] = session;

//    try
//    {
//        await next();

//        if (context.Response.StatusCode < 400)
//        {
//            session.SaveChanges();
//        }
//    }
//    finally
//    {
//        session?.Dispose();
//    }
//});

app.Use(async (context, next) =>
{
    var session = context.RequestServices.GetRequiredService<IDocumentSession>();

    await next();

    if (context.Response.StatusCode < 400 && context.Request.Method != "GET")
    {
        session.SaveChanges();
    }
});

// Map controller routes - Use MapAreaControllerRoute for Admin area
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Posts}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Posts}/{action=Index}/{id?}");

app.Run();

// Custom JSON TempData Serializer to replace BSON serializer
public class JsonTempDataSerializer : TempDataSerializer
{
    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Include
    };

    public override IDictionary<string, object> Deserialize(byte[] value)
    {
        if (value == null || value.Length == 0)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        var json = Encoding.UTF8.GetString(value);
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, Settings)
            ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    public override byte[] Serialize(IDictionary<string, object> values)
    {
        if (values == null || values.Count == 0)
        {
            return Array.Empty<byte>();
        }

        var json = JsonConvert.SerializeObject(values, Settings);
        return Encoding.UTF8.GetBytes(json);
    }
}
