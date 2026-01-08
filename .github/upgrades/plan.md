# RaccoonBlog .NET Framework 4.7.1 to .NET 8.0 Migration Plan

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
  - [RaccoonBlog.Web](#raccoonblogweb)
  - [RaccoonBlog.IntegrationTests](#raccoonblogintegrationtests)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description
Migration of RaccoonBlog solution from **.NET Framework 4.7.1** to **.NET 8.0 (Long Term Support)**, involving a complete architectural transformation from ASP.NET MVC 5 to ASP.NET Core 8.0.

### Scope

**Projects Affected**: 2 projects
- **RaccoonBlog.Web** - Main ASP.NET MVC 5 web application (19,530 LOC)
- **RaccoonBlog.IntegrationTests** - Integration test project (708 LOC)

**Current State**: 
- Target Framework: .NET Framework 4.7.1
- Web Framework: ASP.NET MVC 5.2.3
- Project Style: Classic (non-SDK-style) `.csproj` files
- Total Codebase: 20,238 lines of code across 257 files

**Target State**:
- Target Framework: .NET 8.0
- Web Framework: ASP.NET Core 8.0
- Project Style: SDK-style `.csproj` files
- Modern authentication: ASP.NET Core Identity

### Discovered Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Projects | 2 | All require upgrade |
| Total NuGet Packages | 83 | 42 need upgrade/removal |
| Incompatible Packages | 34 | 41.0% - Require replacement |
| Code Files | 257 | 105 files have API issues |
| Total Lines of Code | 20,238 | |
| **Estimated LOC Impact** | **2,597+** | **12.8% of codebase** |
| Binary Incompatible APIs | 2,267 | Require code changes |
| Source Incompatible APIs | 330 | Need recompilation/fixes |

### Complexity Classification

**Classification: Complex Solution** (High-Risk Migration)

**Rationale**:
- ? Small project count (2 projects) - favorable
- ? Simple dependency structure (depth = 1) - favorable
- ? **Major architectural change**: ASP.NET MVC ? ASP.NET Core (98.2% of API issues)
- ? **High API incompatibility**: 2,597+ lines requiring changes
- ? **Security vulnerabilities**: 4 packages with known CVEs
- ? **Significant package incompatibility**: 34 packages need replacement
- ? **Framework paradigm shift**: System.Web ? ASP.NET Core patterns

### Critical Issues

#### Security Vulnerabilities (MUST ADDRESS)
- **bootstrap** 3.3.1 ? 5.3.8 (multiple high-severity CVEs)
- **jQuery** 1.11.2 ? 3.7.1 (multiple high-severity CVEs)
- **jQuery.UI.Combined** 1.11.2 ? 1.14.1 (high-severity CVEs)
- **jQuery.Validation** 1.13.1 ? 1.21.0 (high-severity CVEs)

#### Architectural Challenges
- **System.Web dependency**: 2,550 API incompatibilities (98.2%)
- **OWIN middleware**: Migration to native ASP.NET Core middleware
- **ASP.NET Identity**: Migration from ASP.NET Identity 2.x to ASP.NET Core Identity
- **Global.asax**: Conversion to Program.cs/Startup.cs pattern
- **Bundling/Minification**: Replacement of System.Web.Optimization
- **Route registration**: RouteCollection ? ASP.NET Core routing

### Selected Strategy

**All-At-Once Strategy** - Both projects upgraded simultaneously in single operation.

**Rationale**:
- Small solution (2 projects) suits atomic upgrade
- Simple dependency chain (Web ? Tests)
- Test project depends on Web, must migrate together to avoid multi-targeting complexity
- Single comprehensive validation phase more efficient than incremental
- Security vulnerabilities best addressed in one coordinated fix

### Iteration Strategy

Given the complexity classification, this plan will use **Phase-Based Detail Generation**:
- **Phase 1 (Foundation)**: Dependency analysis, migration strategy, project stubs, risk overview
- **Phase 2 (Web Application Detail)**: Complete RaccoonBlog.Web migration plan (high-risk, high-complexity)
- **Phase 3 (Test Project Detail)**: Complete RaccoonBlog.IntegrationTests migration plan
- **Phase 4 (Supporting Sections)**: Package updates, breaking changes, testing strategy, success criteria

**Expected Iterations**: 6-7 iterations total

---

## Migration Strategy

### Approach Selection

**Selected Strategy: All-At-Once Strategy**

All projects in the solution will be upgraded simultaneously in a single atomic operation. Both project files will be converted to SDK-style and updated to target net8.0, all package references updated, and all compilation errors fixed in one coordinated batch before moving to validation.

### All-At-Once Strategy Rationale

#### Why All-At-Once is Appropriate

**Small Solution Scale**:
- Only 2 projects total
- Simple linear dependency (Web ? Tests)
- Total codebase: ~20K LOC
- Overhead of incremental phasing not justified

**Tight Project Coupling**:
- IntegrationTests directly depends on RaccoonBlog.Web
- Multi-targeting would require maintaining two parallel API surfaces
- Test project has no value without Web project on same framework
- Shared package versions (RavenDB, AutoMapper, HtmlAgilityPack)

**Architectural Coherence**:
- Single technology stack (ASP.NET MVC ? ASP.NET Core)
- Breaking changes affect both projects identically
- Unified authentication/authorization model
- Shared configuration patterns (web.config ? appsettings.json)

**Risk Factors Favor Atomic Approach**:
- Security vulnerabilities in Web project need immediate resolution
- Test infrastructure must align with Web framework version
- Incremental approach would prolong security exposure
- Single validation phase more efficient than multi-stage

**Unified Testing**:
- Integration tests validate entire Web application
- Cannot meaningfully test with mixed framework versions
- Consolidated test run provides complete confidence
- Single pass/fail signal clearer than incremental results

### Dependency-Based Ordering Principles

While execution is atomic, internal sequencing follows dependency order:

**Phase 1: Project File Conversion (Simultaneous)**
1. Convert RaccoonBlog.Web to SDK-style
2. Convert RaccoonBlog.IntegrationTests to SDK-style
3. Update both to target net8.0

**Phase 2: Package Updates (Simultaneous)**
1. Update/replace packages in RaccoonBlog.Web
2. Update/replace packages in RaccoonBlog.IntegrationTests
3. Remove incompatible packages from both

**Phase 3: Code Changes (Sequential by Dependency)**
1. Fix RaccoonBlog.Web compilation errors (blocking)
2. Fix RaccoonBlog.IntegrationTests compilation errors (dependent)

**Phase 4: Build Validation (Sequential by Dependency)**
1. Build RaccoonBlog.Web successfully (blocking)
2. Build RaccoonBlog.IntegrationTests successfully (dependent)

**Phase 5: Test Execution**
1. Run all integration tests
2. Validate end-to-end scenarios

### Parallel vs Sequential Execution Decisions

| Activity | Execution Mode | Rationale |
|----------|---------------|-----------|
| Project file conversion | Parallel | Independent operations, no code interaction |
| TargetFramework updates | Parallel | Simple property changes, no dependencies |
| Package reference updates | Parallel | Package managers handle resolution |
| Code API migrations | Sequential | Tests depend on Web APIs being available |
| Build validation | Sequential | Tests require Web assembly to reference |
| Test execution | Sequential | Tests require successful Web build |

### Phase Definitions

This All-At-Once migration is structured into a single comprehensive phase with clear sub-operations:

#### Phase 1: Atomic Framework and Package Upgrade

**Scope**: All projects simultaneously

**Operations**:
1. **Project Conversion**
   - Convert both projects to SDK-style .csproj
   - Update TargetFramework to net8.0
   - Remove obsolete project elements

2. **Package Updates**
   - Update all compatible packages to .NET 8-compatible versions
   - Replace incompatible packages with .NET 8 equivalents
   - Remove packages now included in framework
   - Add new ASP.NET Core packages

3. **Dependency Restoration**
   - Run `dotnet restore` for entire solution
   - Resolve any package conflicts

4. **Compilation and Code Fixes**
   - Build RaccoonBlog.Web
   - Fix all breaking changes in Web project
   - Build RaccoonBlog.IntegrationTests
   - Fix all breaking changes in Tests project
   - Rebuild entire solution

5. **Validation**
   - Solution builds with 0 errors
   - 0 critical warnings

**Deliverables**:
- Both projects successfully compile
- No build errors or critical warnings
- All dependencies resolved

#### Phase 2: Test Validation

**Scope**: RaccoonBlog.IntegrationTests

**Operations**:
1. Execute all integration tests
2. Address test failures related to:
   - ASP.NET Core test infrastructure changes
   - RavenDB embedded test driver updates
   - MVC test helper compatibility
3. Re-run tests until all pass

**Deliverables**:
- All integration tests pass
- No test infrastructure errors

### Project-by-Project Migration Plans

This section provides detailed migration specifications for each project in dependency order (bottom-up).

---

### RaccoonBlog.Web

**Project Type**: ASP.NET MVC 5 Web Application (Wap)  
**Current Target Framework**: net471  
**Proposed Target Framework**: net8.0  
**Current SDK Style**: Classic (non-SDK-style)  
**Migration Complexity**: ?? High

#### Current State

- **Lines of Code**: 19,530
- **Files**: 866 total, 97 files with API incompatibilities
- **Estimated LOC Impact**: 2,417+ lines (12.4% of project)
- **Dependencies**: 0 project dependencies
- **Dependants**: 1 (RaccoonBlog.IntegrationTests)
- **Package Count**: 73 packages
- **Package Issues**: 57 packages need upgrade/replacement/removal
- **API Incompatibilities**:
  - 2,100 binary incompatible APIs
  - 317 source incompatible APIs
  - 0 behavioral changes flagged

#### Target State

- **Target Framework**: net8.0
- **SDK Style**: SDK-style .csproj
- **Web Framework**: ASP.NET Core 8.0 MVC
- **Updated Packages**: 42 packages updated/replaced/removed
- **Authentication**: ASP.NET Core Identity with external providers
- **Configuration**: appsettings.json-based configuration
- **Hosting**: Kestrel with Program.cs entry point

#### Migration Steps

##### 1. Prerequisites

**Verify .NET 8 SDK Installation**:
- Confirm .NET 8 SDK installed on development machine
- Verify `dotnet --list-sdks` shows 8.0.x version

**Backup Current State**:
- Current branch `upgrade-to-NET8-3` already isolated from main
- Optionally create additional backup branch: `upgrade-to-NET8-3-backup`
- Backup RavenDB.Embedded data directory if database files exist

##### 2. Convert Project to SDK-Style

**Action**: Convert `RaccoonBlog.Web.csproj` from classic to SDK-style format.

**Specific Changes**:

**Remove Elements** (no longer needed in SDK-style):
- All `<Compile Include="...">` elements (auto-included by default)
- All `<Content Include="...">` for standard patterns (wwwroot auto-included)
- All `<None Include="...">` for standard patterns
- `<Import Project="$(MSBuildToolsPath)\...">` statements
- Assembly info attributes (auto-generated or move to .csproj)
- `<ProjectGuid>`, `<ProjectTypeGuids>`, `<VSToolsPath>`, `<IISExpressSSLPort>`, etc.

**Replace Root Element**:
```xml
<!-- Old -->
<Project ToolsVersion="..." DefaultTargets="Build" xmlns="...">

<!-- New -->
<Project Sdk="Microsoft.NET.Sdk.Web">
```
**Update Core Properties**:
```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <RootNamespace>RaccoonBlog.Web</RootNamespace>
  <AssemblyName>RaccoonBlog.Web</AssemblyName>
</PropertyGroup>
```
**Keep Custom Elements**:
- Preserve any custom MSBuild targets
- Preserve non-standard content includes (if any)
- Preserve project references (will be updated separately)

**Tool**: Consider using .NET Upgrade Assistant or manual conversion following SDK-style template.

##### 3. Update Package References

**Remove Incompatible Packages** (functionality replaced by framework or obsolete):

| Package | Reason for Removal |
|---------|-------------------|
| Microsoft.AspNet.Mvc | Included in Microsoft.AspNetCore.App framework reference |
| Microsoft.AspNet.Razor | Included in framework |
| Microsoft.AspNet.WebPages | Included in framework |
| Microsoft.Web.Infrastructure | Included in framework |
| System.Buffers | Included in framework |
| System.Memory | Included in framework |
| System.Numerics.Vectors | Included in framework |
| System.Runtime.InteropServices.RuntimeInformation | Included in framework |
| System.Security.Cryptography.Cng | Included in framework |
| System.Threading.Tasks.Extensions | Included in framework |
| Microsoft.Bcl | Obsolete for .NET 8 |
| Microsoft.Bcl.Build | Obsolete for .NET 8 |

**Replace Incompatible Packages**:

| Old Package | Current Version | New Package | Target Version | Reason |
|-------------|----------------|-------------|----------------|---------|
| Antlr | 3.5.0.2 | Antlr4 | 4.6.6 | Antlr3 incompatible, Antlr4 for .NET Standard/Core |
| Microsoft.Net.Http | 2.2.29 | System.Net.Http | 4.3.4 | Old BCL package replaced |
| Microsoft.AspNet.Identity.Core | 2.2.4 | Microsoft.AspNetCore.Identity | 8.0.x | ASP.NET Core Identity |
| Microsoft.AspNet.Identity.Owin | 2.2.4 | Microsoft.AspNetCore.Identity | 8.0.x | ASP.NET Core Identity |
| Microsoft.Owin.* (all 8 packages) | various | (Remove) | - | OWIN replaced by ASP.NET Core middleware |
| Owin | 1.0 | (Remove) | - | OWIN replaced by ASP.NET Core middleware |
| AttributeRouting.* (3 packages) | 3.5.6 | (Remove) | - | ASP.NET Core has built-in attribute routing |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | (Remove or alternative) | - | Use direct script tags or WebOptimizer package |
| DataAnnotationsExtensions | 5.0.1.20 | (Remove) | - | Functionality in System.ComponentModel.DataAnnotations |
| DataAnnotationsExtensions.MVC3 | 5.0.1.20 | (Remove) | - | Functionality in framework |
| DotNetOpenAuth.Core | 4.3.4.13329 | (Evaluate alternative) | - | OAuth handled by ASP.NET Core authentication |
| DotNetOpenAuth.Mvc5 | 4.3.4.13329 | (Evaluate alternative) | - | OAuth handled by ASP.NET Core authentication |
| MvcDonutCaching | 1.3.1-beta1 | (Remove or alternative) | - | Use ASP.NET Core response caching |
| RazorEngine | 3.10.0 | (Remove or alternative) | - | ASP.NET Core Razor already built-in |
| T4MVCExtensions | 4.1.0 | (Remove) | - | T4MVC may work; extensions obsolete |
| WebActivator | 1.5 | (Remove) | - | Startup logic moves to Program.cs |
| WebActivatorEx | 2.2.0 | (Remove) | - | Startup logic moves to Program.cs |
| xmlrpcnet | 2.5.0 | (Evaluate alternative) | - | Unmaintained; use CookComputing.XmlRpcV2 or alternatives |
| System.Web.Optimization.Less | 1.3.4 | (Remove or alternative) | - | Use build-time LESS compiler |
| Validation | 2.0.2.13022 | (Remove) | - | Functionality in System.ComponentModel.DataAnnotations |
| RedditSharp | 1.1.14 | RedditSharp | 2.0.0 | Upgrade to 2.0 (API changes likely) |
| RhinoMocks | 3.6.1 | (Remove or alternative) | - | Consider Moq or NSubstitute for tests |
| NLog | 4.4.12 | NLog.Web.AspNetCore | 6.0.7 | ASP.NET Core-compatible NLog |

**Update Compatible Packages to Recommended Versions**:

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| bootstrap | 3.3.1 | 5.3.8 | **SECURITY VULNERABILITY** |
| jQuery | 1.11.2 | 3.7.1 | **SECURITY VULNERABILITY** |
| jQuery.UI.Combined | 1.11.2 | 1.14.1 | **SECURITY VULNERABILITY** |
| jQuery.Validation | 1.13.1 | 1.21.0 | **SECURITY VULNERABILITY** |
| Microsoft.AspNetCore.JsonPatch | 8.0.16 | 8.0.22 | Security/bug fixes |
| Microsoft.Bcl.AsyncInterfaces | 9.0.5 | 8.0.0 | Align with .NET 8 |
| System.Collections.Immutable | 9.0.5 | 8.0.0 | Align with .NET 8 |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Security/bug fixes |

**Add New ASP.NET Core Packages**:

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.Facebook | 8.0.x | Facebook authentication |
| Microsoft.AspNetCore.Authentication.Google | 8.0.x | Google authentication |
| Microsoft.AspNetCore.Authentication.MicrosoftAccount | 8.0.x | Microsoft authentication |
| Microsoft.AspNetCore.Authentication.Twitter | 8.0.x | Twitter authentication |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.x | If using EF Core for Identity storage |
| Microsoft.Extensions.Configuration.Json | 8.0.x | JSON configuration support |
| Microsoft.Extensions.Logging.Console | 8.0.x | Console logging provider |

**Keep Compatible Packages** (no changes needed):
- AutoMapper 6.2.2
- dotless 1.5.2
- FluentScheduler 5.3.0
- HtmlAgilityPack 1.6.16
- JetBrains.Annotations 11.1.0
- jQuery.Migrate 1.2.1
- jQuery.Templates 0.1
- Lambda2Js.Signed 3.1.4
- MarkdownDeep.Full 1.5
- Microsoft.CSharp 4.7.0
- Microsoft.Bcl.HashCode 6.0.0
- Microsoft.IO.RecyclableMemoryStream 3.0.1
- Nito.AsyncEx.* (all packages)
- Nito.Collections.Deque 1.1.1
- Nito.Disposables 2.2.1
- RavenDB.Client 7.1.0-rc-71000
- RavenDB.Embedded 6.0.104
- RavenDB.TestDriver 6.0.104
- System.Runtime.CompilerServices.Unsafe 6.1.2
- T4MVC 4.1.0
- Twitter.Bootstrap.Less 3.3.2
- WebGrease 1.6.0

**Deprecated Packages to Remove** (no longer maintained, low-risk):
- Microsoft.jQuery.Unobtrusive.Validation 3.2.3 (use ASP.NET Core validation)
- Moment.js 2.30.1 (deprecated, use alternative or CDN)

##### 4. Expected Breaking Changes

**A. ASP.NET MVC ? ASP.NET Core MVC**

**Controllers**:
- Change base class: `System.Web.Mvc.Controller` ? `Microsoft.AspNetCore.Mvc.Controller`
- Update action result types:
  - `ActionResult` ? `IActionResult` or `ActionResult<T>`
  - `ViewResult` ? `IActionResult` (return type)
  - `RedirectToRouteResult` ? `RedirectToRouteResult` (different namespace)
  - `HttpNotFoundResult` ? `NotFoundResult`
- Update HTTP attributes:
  - `[HttpGet]`, `[HttpPost]` same name, different namespace: `Microsoft.AspNetCore.Mvc`
  - `[ChildActionOnly]` ? Remove (no child actions in ASP.NET Core, use View Components)
  - `[NonAction]` ? Same name, different namespace
- Update controller properties:
  - `Request` ? Remains but type changes to `HttpRequest` (Microsoft.AspNetCore.Http)
  - `Response` ? Type changes to `HttpResponse` (Microsoft.AspNetCore.Http)
  - `ModelState` ? Remains same name, different namespace
  - `ViewBag` ? Remains
  - `Url` ? Remains, but type is `IUrlHelper`
- Remove `HttpContext.Current` static access ? Use `HttpContext` property instead

**Views (Razor)**:
- Update `@using` directives:
  - `System.Web.Mvc` ? `Microsoft.AspNetCore.Mvc`
  - `System.Web.Routing` ? `Microsoft.AspNetCore.Routing`
- Update HTML helpers:
  - `MvcHtmlString` ? `IHtmlContent`
  - `MvcHtmlString.Create(...)` ? `new HtmlString(...)`
  - Most `@Html` helpers remain similar but in different namespace
- Ajax helpers:
  - `System.Web.Mvc.Ajax` ? Use JavaScript fetch or jQuery AJAX directly
  - `AjaxRequestExtensions.IsAjaxRequest()` ? Check `Request.Headers["X-Requested-With"] == "XMLHttpRequest"`

**Routing**:
- Update attribute routing:
  - AttributeRouting package incompatible ? Use built-in `[Route]` attribute
  - `[GET]`, `[POST]` from AttributeRouting ? Use `[HttpGet]`, `[HttpPost]`
- Update conventional routes (Global.asax ? Program.cs):
  - `RouteCollection` ? Use `IEndpointRouteBuilder` in Program.cs
  - `routes.MapRoute(...)` ? `endpoints.MapControllerRoute(...)`

**Filters**:
- Update filter attributes:
  - `GlobalFilterCollection` ? Add filters via `.AddControllersWithViews(options => options.Filters.Add(...))`
  - `System.Web.Mvc.IActionFilter` ? `Microsoft.AspNetCore.Mvc.Filters.IActionFilter`
  - `System.Web.Mvc.AuthorizeAttribute` ? `Microsoft.AspNetCore.Authorization.AuthorizeAttribute`
- Custom filters: Update interface implementations to ASP.NET Core equivalents

**Model Binding & Validation**:
- `ModelStateDictionary` ? Same name, different namespace (`Microsoft.AspNetCore.Mvc.ModelBinding`)
- `ModelState.AddModelError(...)` ? Same signature
- `ModelState.IsValid` ? Same
- Data annotations remain mostly the same

**B. Configuration System**

**web.config ? appsettings.json**:
- Move `<appSettings>` ? `appsettings.json` root or nested sections
- Move `<connectionStrings>` ? `"ConnectionStrings"` section in appsettings.json
- Remove `ConfigurationManager.AppSettings[...]` ? Use `IConfiguration` injected in controllers/services
- Remove `ConfigurationManager.ConnectionStrings[...]` ? Use `IConfiguration.GetConnectionString(...)`

**Example transformation**:
```xml
<!-- web.config -->
<appSettings>
  <add key="BlogName" value="RaccoonBlog" />
</appSettings>
```
```json
// appsettings.json
{
  "BlogName": "RaccoonBlog"
}
```

**C. Authentication & Authorization**

**OWIN ? ASP.NET Core Middleware**:
- Remove `Startup.cs` OWIN configuration
- Create new `Program.cs` with ASP.NET Core host builder
- Migrate authentication configuration:
  - `app.UseCookieAuthentication(...)` ? `builder.Services.AddAuthentication().AddCookie(...)`
  - `app.UseExternalSignInCookie(...)` ? Configure external authentication schemes
  - External providers (Facebook, Google, Twitter, Microsoft):
    - Update to ASP.NET Core authentication packages
    - `app.UseFacebookAuthentication(...)` ? `builder.Services.AddAuthentication().AddFacebook(...)`
    - Similar for Google, Twitter, Microsoft Account

**ASP.NET Identity 2.x ? ASP.NET Core Identity**:
- Update `UserManager`, `SignInManager` to ASP.NET Core Identity versions
- Update user/role classes if custom Identity models used
- Update Identity configuration in `Program.cs`:
  ```csharp
  builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddDefaultTokenProviders();
  ```
- Update password hashing (ASP.NET Core Identity uses different hasher - may need compat)

**Cookie Authentication**:
- Update cookie authentication options:
  - `CookieAuthenticationOptions` ? `CookieAuthenticationOptions` (different namespace)
  - `LoginPath`, `LogoutPath`, `AccessDeniedPath` remain similar

**D. Application Initialization**

**Global.asax ? Program.cs**:

- Remove `Global.asax.cs` file
- Create `Program.cs` as entry point:
  ```csharp
  var builder = WebApplication.CreateBuilder(args);
  
  // Add services (formerly in Application_Start)
  builder.Services.AddControllersWithViews();
  builder.Services.AddRazorPages();
  // ... other service registrations
  
  var app = builder.Build();
  
  // Configure middleware pipeline (formerly in Application_Start)
  if (app.Environment.IsDevelopment())
  {
      app.UseDeveloperExceptionPage();
  }
  else
  {
      app.UseExceptionHandler("/Home/Error");
      app.UseHsts();
  }
  
  app.UseHttpsRedirection();
  app.UseStaticFiles();
  app.UseRouting();
  app.UseAuthentication();
  app.UseAuthorization();
  
  app.MapControllerRoute(
      name: "default",
      pattern: "{controller=Home}/{action=Index}/{id?}");
  
  app.Run();
  ```

- Migrate `Application_Start()` logic:
  - Dependency injection setup ? `builder.Services.Add*(...)`
  - AutoMapper configuration ? Register in DI
  - FluentScheduler initialization ? Configure in Program.cs after app built
  - Logging configuration ? Use `builder.Logging.Add*(...)`

- Migrate `Application_Error()` ? Use exception handling middleware
- Migrate `Application_BeginRequest()` ? Use custom middleware
- Migrate `Application_EndRequest()` ? Use custom middleware

**E. Bundling & Minification**

**System.Web.Optimization ? Alternative**:

Option 1: **Remove bundling** (simplest for migration):
- Remove `BundleConfig.cs`
- Update `_Layout.cshtml` and views:
  - Replace `@Scripts.Render("~/bundles/jquery")` with direct `<script src="...">` tags
  - Replace `@Styles.Render("~/Content/css")` with direct `<link href="...">` tags
- Serve scripts/styles from `wwwroot/js` and `wwwroot/css`

Option 2: **Use WebOptimizer** (modern alternative):
- Add package: `LigerShark.WebOptimizer.Core`
- Configure in Program.cs:
  ```csharp
  builder.Services.AddWebOptimizer(pipeline =>
  {
      pipeline.AddCssBundle("/css/bundle.css", "css/site.css", "css/other.css");
      pipeline.AddJavaScriptBundle("/js/bundle.js", "js/site.js", "js/other.js");
  });
  ```
- Update views to reference bundle paths

**Recommendation**: Option 1 for initial migration (reduce complexity), evaluate Option 2 post-migration.

**F. Static Files & wwwroot**

- Create `wwwroot` folder if doesn't exist
- Move static content:
  - `Content/**` ? `wwwroot/css/**`
  - `Scripts/**` ? `wwwroot/js/**`
  - `Images/**` ? `wwwroot/images/**`
  - `fonts/**` ? `wwwroot/fonts/**`
- Update references in views to new paths (without `~Content/`, just `/css/...`)
- Add `app.UseStaticFiles()` in Program.cs

**G. Dependency Injection**

- Remove manual dependency resolution (if using custom IoC)
- Use built-in ASP.NET Core DI:
  - Register services in `Program.cs`: `builder.Services.AddScoped<IService, Service>()`
  - Controllers automatically get constructor injection
  - Views can use `@inject IService Service`

**H. Logging**

- Remove NLog configuration from web.config (if present)
- Configure NLog for ASP.NET Core:
  - Add `nlog.config` file in project root
  - Configure in Program.cs:
    ```csharp
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Host.UseNLog();
    ```
- Update log calls: `ILogger<T>` injected via DI

**I. RavenDB Integration**

- RavenDB.Client 7.1.0-rc-71000 should be compatible with .NET 8
- RavenDB.Embedded 6.0.104 should be compatible
- Update document store initialization to use ASP.NET Core DI
- Register as singleton:
  ```csharp
  builder.Services.AddSingleton<IDocumentStore>(provider =>
  {
      var store = new DocumentStore { /* configuration */ };
      store.Initialize();
      return store;
  });
  ```
- Update session management pattern if using per-request sessions

**J. Legacy Cryptography**

- Replace obsolete cryptography APIs (5 instances identified):
  - Review usage of deprecated algorithms
  - Update to modern cryptography APIs
  - Consult breaking changes documentation for specific replacements

##### 5. Code Modifications

**Areas Requiring Code Changes** (organized by priority):

**Priority 1: Critical Path (Required for Compilation)**

1. **Update all controller files** (97 files with issues):
   - Change `using System.Web.Mvc;` ? `using Microsoft.AspNetCore.Mvc;`
   - Change `using System.Web.Routing;` ? `using Microsoft.AspNetCore.Routing;`
   - Update action result types
   - Update HTTP method attributes
   - Remove `[ChildActionOnly]` attributes
   - Update `UrlHelper` usage

2. **Create Program.cs**:
   - Replace Global.asax application initialization
   - Configure services (DI, authentication, MVC, etc.)
   - Configure middleware pipeline
   - Set up routing

3. **Update Views** (_Layout.cshtml, _ViewStart.cshtml, all view files):
   - Update `@using` directives
   - Update HTML helper calls if needed
   - Update script/style bundle references (remove bundling)
   - Update Ajax helper usage

4. **Configuration Migration**:
   - Create `appsettings.json` with app settings and connection strings
   - Update code accessing `ConfigurationManager` to use `IConfiguration`

5. **Authentication & Identity**:
   - Update Startup authentication configuration to Program.cs
   - Update Identity-related code (user manager, sign-in manager)
   - Update authentication controller actions
   - Update external provider configurations

**Priority 2: Important (Required for Functionality)**

6. **Remove/Replace AttributeRouting**:
   - Replace custom route attributes with standard `[Route]`, `[HttpGet]`, `[HttpPost]`
   - Verify all routes work with ASP.NET Core attribute routing

7. **Update Filter Usage**:
   - Update custom filter implementations
   - Update global filter registration to Program.cs

8. **Logging Updates**:
   - Update NLog configuration for ASP.NET Core
   - Update logger usage patterns to `ILogger<T>`

9. **RavenDB Registration**:
   - Move document store initialization to Program.cs DI
   - Update session management

10. **Static Files Organization**:
    - Create wwwroot folder structure
    - Move static content
    - Update view references

**Priority 3: Cleanup & Optimization (Post-Compilation)**

11. **Remove Obsolete Code**:
    - Remove Global.asax
    - Remove BundleConfig.cs (if removing bundling)
    - Remove AttributeRouting configurations
    - Remove OWIN Startup.cs

12. **Update Data Annotations**:
    - Remove DataAnnotationsExtensions usage
    - Use standard data annotations

13. **Cryptography Updates**:
    - Replace obsolete cryptography API calls

##### 6. Testing Strategy

**Build Validation**:
- [ ] RaccoonBlog.Web project builds without errors
- [ ] RaccoonBlog.Web project builds without critical warnings
- [ ] No package restore errors
- [ ] No dependency conflicts

**Smoke Testing**:
- [ ] Application starts without exceptions
- [ ] Home page loads successfully
- [ ] Static files (CSS, JS, images) load correctly
- [ ] RavenDB embedded database initializes

**Authentication Testing**:
- [ ] Login with local account works
- [ ] Logout works
- [ ] Register new account works
- [ ] External authentication providers configured (may need testing with credentials)

**Functionality Testing**:
- [ ] Blog post listing displays
- [ ] Blog post detail view displays
- [ ] Admin functions accessible (if applicable)
- [ ] Search functionality works
- [ ] Comment posting works
- [ ] RSS feed generation works

**Security Validation**:
- [ ] jQuery updated to 3.7.1 (verify in browser dev tools)
- [ ] Bootstrap updated to 5.3.8
- [ ] jQuery UI updated to 1.14.1
- [ ] jQuery Validation updated to 1.21.0
- [ ] No security scan warnings for vulnerable packages

##### 7. Validation Checklist

- [ ] Project converted to SDK-style .csproj
- [ ] TargetFramework set to net8.0
- [ ] All incompatible packages removed/replaced
- [ ] All security-vulnerable packages updated
- [ ] Program.cs created with proper middleware pipeline
- [ ] appsettings.json created with configuration values
- [ ] wwwroot folder created with static files
- [ ] Global.asax removed
- [ ] All controller files updated with ASP.NET Core MVC namespaces
- [ ] All view files updated with ASP.NET Core namespaces
- [ ] Authentication configured for ASP.NET Core Identity
- [ ] External authentication providers configured
- [ ] Routing configured (attribute and conventional)
- [ ] Logging configured (NLog for ASP.NET Core)
- [ ] RavenDB document store registered in DI
- [ ] Build succeeds with 0 errors
- [ ] Build succeeds with 0 critical warnings
- [ ] Application starts successfully
- [ ] All smoke tests pass
- [ ] No security vulnerabilities in packages

---

### RaccoonBlog.IntegrationTests

**Project Type**: xUnit Test Project (ClassicClassLibrary)  
**Current Target Framework**: net471  
**Proposed Target Framework**: net8.0  
**Current SDK Style**: Classic (non-SDK-style)  
**Migration Complexity**: ?? Medium

#### Current State

- **Lines of Code**: 708
- **Files**: 12 total, 8 files with API incompatibilities
- **Estimated LOC Impact**: 180+ lines (25.4% of project)
- **Dependencies**: 1 (RaccoonBlog.Web)
- **Dependants**: 0
- **Package Count**: 10 packages
- **Package Issues**: 5 packages need upgrade/replacement
- **API Incompatibilities**:
  - 167 binary incompatible APIs
  - 13 source incompatible APIs
  - 0 behavioral changes flagged

#### Target State

- **Target Framework**: net8.0
- **SDK Style**: SDK-style .csproj
- **Test Framework**: xUnit 2.4.0 (compatible with .NET 8)
- **Updated Packages**: 5 packages updated/replaced/removed
- **Test Infrastructure**: ASP.NET Core test infrastructure

#### Migration Steps

##### 1. Prerequisites

**Dependency Requirement**:
- RaccoonBlog.Web **must** be successfully migrated and building before this project can be migrated
- This project references RaccoonBlog.Web assembly - cannot build without it

**Verify .NET 8 SDK**:
- Same as RaccoonBlog.Web (already verified)

##### 2. Convert Project to SDK-Style

**Action**: Convert `RaccoonBlog.IntegrationTests.csproj` from classic to SDK-style format.

**Specific Changes**:

**Replace Root Element**:
```xml
<!-- Old -->
<Project ToolsVersion="..." DefaultTargets="Build" xmlns="...">

<!-- New -->
<Project Sdk="Microsoft.NET.Sdk">
```

**Update Core Properties**:
```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <IsPackable>false</IsPackable>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <RootNamespace>RaccoonBlog.IntegrationTests</RootNamespace>
  <AssemblyName>RaccoonBlog.IntegrationTests</AssemblyName>
</PropertyGroup>
```
**Remove Elements** (auto-included or obsolete):
- All `<Compile Include="...">` elements
- All `<None Include="...">` for standard patterns
- `<Import Project="$(MSBuildToolsPath)\...">` statements
- `<ProjectGuid>`, assembly info references, etc.

**Update Project Reference**:
```xml
<ItemGroup>
  <ProjectReference Include="..\RaccoonBlog.Web\RaccoonBlog.Web.csproj" />
</ItemGroup>
```

##### 3. Update Package References

**Remove Incompatible Packages**:

| Package | Current Version | Reason for Removal |
|---------|----------------|-------------------|
| Microsoft.AspNet.Mvc | 5.2.3 | Included in ASP.NET Core framework (via RaccoonBlog.Web reference) |
| RhinoMocks | 3.6.1 | Incompatible with .NET 8; replace with modern mocking library |

**Replace Incompatible Packages**:

| Old Package | Current Version | New Package | Target Version | Reason |
|-------------|----------------|-------------|----------------|---------|
| RedditSharp | 1.1.14 | RedditSharp | 2.0.0 | Update to .NET Standard-compatible version |
| RhinoMocks | 3.6.1 | Moq or NSubstitute | Latest | RhinoMocks unmaintained; use modern mock framework |

**Recommendation**: Replace RhinoMocks with **Moq** (most popular):
```xml
<PackageReference Include="Moq" Version="4.20.70" />
```

**Keep Compatible Packages** (no changes needed):
- AutoMapper 6.2.2 ?
- HtmlAgilityPack 1.6.16 ?
- RavenDB.Client 6.0.104 ?
- RavenDB.TestDriver 6.0.104 ?
- xUnit 2.4.0 ?
- xunit.analyzers 0.10.0 ?
- xunit.core 2.4.0 ?
- MvcContrib.Mvc3.TestHelper-ci 3.0.100 ? (may need runtime verification)

##### 4. Expected Breaking Changes

**A. ASP.NET MVC Test Helpers ? ASP.NET Core Test Infrastructure**

**MvcContrib.Mvc3.TestHelper Compatibility**:
- Package is marked compatible, but may have runtime issues
- If issues arise, consider:
  - Using ASP.NET Core `TestServer` and `WebApplicationFactory<T>`
  - Refactoring tests to use modern ASP.NET Core test patterns

**Test Infrastructure Updates**:
- Update controller test setup to work with ASP.NET Core controllers
- Update HttpContext mocking (different types in ASP.NET Core)
- Update routing test setup

**B. RhinoMocks ? Moq Migration**

If replacing RhinoMocks with Moq:

**RhinoMocks Pattern**:
```csharp
var mock = MockRepository.GenerateMock<IService>();
mock.Stub(x => x.Method()).Return(value);
mock.AssertWasCalled(x => x.Method());
```

**Moq Pattern**:
```csharp
var mock = new Mock<IService>();
mock.Setup(x => x.Method()).Returns(value);
mock.Verify(x => x.Method(), Times.Once);
```

**Migration Steps**:
- Replace all `MockRepository.GenerateMock<T>()` with `new Mock<T>()`
- Replace `.Stub(...)` with `.Setup(...)`
- Replace `.AssertWasCalled(...)` with `.Verify(...)`
- Replace `.AssertWasNotCalled(...)` with `.Verify(..., Times.Never)`

**C. ASP.NET MVC Controller Testing**

**System.Web.Mvc Dependencies**:
- Update `using System.Web.Mvc;` ? `using Microsoft.AspNetCore.Mvc;`
- Update controller base types
- Update action result types

**HttpContext Mocking**:
- `HttpContext` and `HttpContextBase` from System.Web ? `HttpContext` from Microsoft.AspNetCore.Http
- Different mocking approach may be needed
- Consider using `DefaultHttpContext` for test scenarios

**D. RedditSharp 1.1.14 ? 2.0.0**

- RedditSharp 2.0 has breaking API changes
- Review RedditSharp usage in tests
- Update API calls according to RedditSharp 2.0 documentation
- Likely changes in authentication, subreddit access, post submission patterns

**E. RavenDB TestDriver**

- RavenDB.TestDriver 6.0.104 should be compatible with .NET 8
- Verify test database initialization works
- Update embedded RavenDB configuration if needed

##### 5. Code Modifications

**Priority 1: Critical Path (Required for Compilation)**

1. **Update Test Class Namespaces** (8 files with issues):
   - Change `using System.Web.Mvc;` ? `using Microsoft.AspNetCore.Mvc;`
   - Change `using System.Web;` ? `using Microsoft.AspNetCore.Http;`
   - Add `using Microsoft.AspNetCore.Mvc.Testing;` (if using WebApplicationFactory)

2. **Update Controller Test Setup**:
   - Update controller instantiation for ASP.NET Core
   - Update HttpContext mocking approach
   - Update action result assertions

3. **Migrate RhinoMocks to Moq** (if replacing):
   - Replace all mock creation calls
   - Replace stub/setup patterns
   - Replace assertion patterns
   - Update test assertions

4. **Update RedditSharp Usage**:
   - Review RedditSharp 2.0 API documentation
   - Update authentication patterns
   - Update subreddit/post access patterns
   - Update API method calls

**Priority 2: Important (Required for Tests to Pass)**

5. **Update Test Infrastructure**:
   - Update RavenDB TestDriver initialization if needed
   - Update test database setup/teardown
   - Update AutoMapper configuration for tests

6. **Update Integration Test Setup**:
   - Consider using `WebApplicationFactory<Program>` for integration tests
   - Update test server configuration
   - Update HTTP client usage in tests

7. **Update Assertions**:
   - Verify xUnit assertions still work (should be compatible)
   - Update action result type assertions for ASP.NET Core

**Priority 3: Optimization (Post-Test Passing)**

8. **Modernize Test Patterns**:
   - Consider using ASP.NET Core test host patterns
   - Evaluate test isolation improvements
   - Refactor for better test maintainability

##### 6. Testing Strategy

**Build Validation**:
- [ ] RaccoonBlog.Web project builds without errors
- [ ] RaccoonBlog.Web project builds without critical warnings
- [ ] No package restore errors
- [ ] No dependency conflicts

**Smoke Testing**:
- [ ] Application starts without exceptions
- [ ] Home page loads successfully
- [ ] Static files (CSS, JS, images) load correctly
- [ ] RavenDB embedded database initializes

**Authentication Testing**:
- [ ] Login with local account works
- [ ] Logout works
- [ ] Register new account works
- [ ] External authentication providers configured (may need testing with credentials)

**Functionality Testing**:
- [ ] Blog post listing displays
- [ ] Blog post detail view displays
- [ ] Admin functions accessible (if applicable)
- [ ] Search functionality works
- [ ] Comment posting works
- [ ] RSS feed generation works

**Security Validation**:
- [ ] jQuery updated to 3.7.1 (verify in browser dev tools)
- [ ] Bootstrap updated to 5.3.8
- [ ] jQuery UI updated to 1.14.1
- [ ] jQuery Validation updated to 1.21.0
- [ ] No security scan warnings for vulnerable packages

##### 7. Validation Checklist

- [ ] Project converted to SDK-style .csproj
- [ ] TargetFramework set to net8.0
- [ ] All incompatible packages removed/replaced
- [ ] All security-vulnerable packages updated
- [ ] Program.cs created with proper middleware pipeline
- [ ] appsettings.json created with configuration values
- [ ] wwwroot folder created with static files
- [ ] Global.asax removed
- [ ] All controller files updated with ASP.NET Core MVC namespaces
- [ ] All view files updated with ASP.NET Core namespaces
- [ ] Authentication configured for ASP.NET Core Identity
- [ ] External authentication providers configured
- [ ] Routing configured (attribute and conventional)
- [ ] Logging configured (NLog for ASP.NET Core)
- [ ] RavenDB document store registered in DI
- [ ] Build succeeds with 0 errors
- [ ] Build succeeds with 0 critical warnings
- [ ] Application starts successfully
- [ ] All smoke tests pass
- [ ] No security vulnerabilities in packages

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

The All-At-Once strategy requires comprehensive testing at multiple levels to ensure the migration is successful and complete.

---

### Level 1: Build Validation

**Timing**: After each major code change batch (project conversion, package updates, compilation fixes)

**Scope**: Both projects

#### RaccoonBlog.Web Build Validation

**Validation Criteria**:
- [ ] Project file is valid SDK-style format
- [ ] TargetFramework set to `net8.0`
- [ ] All PackageReferences resolve successfully
- [ ] `dotnet restore` completes without errors
- [ ] `dotnet build` completes without errors
- [ ] Build produces 0 compilation errors
- [ ] Build produces 0 critical warnings (CS#### errors)

**Acceptable Warnings**:
- IDE suggestions (IDE0###)
- Nullable reference type warnings (if incrementally enabling nullability)
- XML documentation warnings (CS1591)

**Build Command**:
```bash
dotnet build RaccoonBlog.Web\RaccoonBlog.Web.csproj --configuration Release
```

#### RaccoonBlog.IntegrationTests Build Validation

**Validation Criteria**:
- [ ] Project file is valid SDK-style format
- [ ] TargetFramework set to `net8.0`
- [ ] ProjectReference to RaccoonBlog.Web resolves
- [ ] All PackageReferences resolve successfully
- [ ] `dotnet restore` completes without errors
- [ ] `dotnet build` completes without errors
- [ ] Build produces 0 compilation errors
- [ ] Build produces 0 critical warnings

**Build Command**:
```bash
dotnet build RaccoonBlog.IntegrationTests\RaccoonBlog.IntegrationTests.csproj --configuration Release
```

#### Solution Build Validation

**Validation Criteria**:
- [ ] Solution builds successfully
- [ ] Both projects build in correct dependency order
- [ ] No package version conflicts

**Build Command**:
```bash
dotnet build RaccoonBlog.sln --configuration Release
```

---

### Level 2: Smoke Testing (Per-Project)

**Timing**: After RaccoonBlog.Web compiles successfully

**Scope**: RaccoonBlog.Web application

#### Application Startup Validation

**Validation Criteria**:
- [ ] Application starts without throwing exceptions
- [ ] Program.cs executes successfully
- [ ] Dependency injection container initializes
- [ ] Middleware pipeline configures correctly
- [ ] Kestrel web server starts
- [ ] Application listens on configured port (default: https://localhost:5001)

**Startup Command**:
```bash
dotnet run --project RaccoonBlog.Web\RaccoonBlog.Web.csproj
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

#### RavenDB Embedded Validation

**Validation Criteria**:
- [ ] RavenDB embedded database initializes
- [ ] Document store creation succeeds
- [ ] Database server starts (embedded mode)
- [ ] No RavenDB connection errors in console

**Check**: Review console output for RavenDB initialization messages.

#### Static Files Validation

**Validation Criteria**:
- [ ] Navigate to `https://localhost:5001/css/site.css` - returns CSS file
- [ ] Navigate to `https://localhost:5001/js/site.js` - returns JavaScript file (if exists)
- [ ] Check browser console for 404 errors on static files
- [ ] Verify Bootstrap CSS loads: `https://localhost:5001/css/bootstrap.css` or CDN reference works

**Check**: Open browser developer tools (F12), check Network tab for failed requests.

#### Home Page Load Validation

**Validation Criteria**:
- [ ] Navigate to `https://localhost:5001` - page loads without error
- [ ] No unhandled exceptions in console
- [ ] No ASP.NET Core error page displayed
- [ ] Page renders (may have styling issues, acceptable at this stage)
- [ ] No JavaScript console errors (critical errors only)

**Acceptable Issues**:
- Styling issues (Bootstrap 3?5 migration)
- Non-critical JavaScript warnings
- Missing images (if paths not updated yet)

#### Basic Navigation Validation

**Validation Criteria**:
- [ ] Navigate to 2-3 different pages (About, Contact, etc.)
- [ ] No routing errors (404s for expected routes)
- [ ] Controllers load and execute actions
- [ ] Views render (even if unstyled)

---

### Level 3: Authentication & Authorization Testing

**Timing**: After smoke tests pass

**Scope**: RaccoonBlog.Web authentication features

#### Local Account Authentication

**Validation Criteria**:
- [ ] Navigate to `/Account/Login` - login page loads
- [ ] Login form displays correctly
- [ ] Submit login form - no server errors
- [ ] Successful login redirects appropriately
- [ ] User identity persists across requests (check cookie)
- [ ] Logout functionality works
- [ ] Navigate to `/Account/Register` - register page loads
- [ ] User registration works

**Test Steps**:
1. Navigate to login page
2. Enter test credentials (or create new account)
3. Verify login succeeds
4. Check authenticated user name displays
5. Navigate to protected resource - access granted
6. Logout
7. Attempt to access protected resource - redirected to login

#### External Authentication Providers

**Validation Criteria**:
- [ ] Facebook login button displays (if configured)
- [ ] Google login button displays (if configured)
- [ ] Twitter login button displays (if configured)
- [ ] Microsoft Account login button displays (if configured)
- [ ] External provider redirect works (requires valid app credentials)

**Note**: Full external authentication testing requires valid OAuth credentials configured in `appsettings.json`. At minimum, verify configuration loads without errors and buttons display.

#### Authorization

**Validation Criteria**:
- [ ] Anonymous users can access public pages
- [ ] Anonymous users redirected from protected pages
- [ ] Authenticated users can access protected pages
- [ ] Admin-only pages respect role restrictions (if applicable)

---

### Level 4: Functionality Testing

**Timing**: After authentication testing passes

**Scope**: Core RaccoonBlog features

#### Blog Post Management

**Validation Criteria**:
- [ ] Blog post listing displays correctly
- [ ] Post detail view loads
- [ ] Post creation form loads (if authenticated)
- [ ] New post can be created
- [ ] Post editing works
- [ ] Post deletion works
- [ ] Post publishing/unpublishing works

#### Comments

**Validation Criteria**:
- [ ] Comments display on post detail page
- [ ] Comment submission form displays
- [ ] New comment can be submitted
- [ ] Comment moderation works (if applicable)

#### Search

**Validation Criteria**:
- [ ] Search form displays
- [ ] Search query submission works
- [ ] Search results display
- [ ] Search integrates with RavenDB correctly

#### RSS Feed

**Validation Criteria**:
- [ ] Navigate to `/rss` or RSS feed URL
- [ ] Valid RSS XML returned
- [ ] Feed contains recent posts

#### Admin Functions

**Validation Criteria**:
- [ ] Admin dashboard loads (if exists)
- [ ] Admin can manage posts
- [ ] Admin can manage comments
- [ ] Admin can manage users (if applicable)

---

### Level 5: Integration Testing (Automated)

**Timing**: After functionality testing passes

**Scope**: RaccoonBlog.IntegrationTests project

#### Test Discovery

**Validation Criteria**:
- [ ] xUnit test runner discovers all tests
- [ ] Test count matches expected (should not lose tests)
- [ ] Tests appear in Visual Studio Test Explorer or `dotnet test` output

**Command**:
```bash
dotnet test RaccoonBlog.IntegrationTests\RaccoonBlog.IntegrationTests.csproj --list-tests
```

#### Test Execution

**Validation Criteria**:
- [ ] All tests execute (not crash on infrastructure errors)
- [ ] RavenDB TestDriver initializes
- [ ] Test web host initializes (if using WebApplicationFactory)
- [ ] Tests can instantiate controllers
- [ ] Mocking framework (Moq) works correctly

**Command**:
```bash
dotnet test RaccoonBlog.IntegrationTests\RaccoonBlog.IntegrationTests.csproj
```

#### Test Pass Criteria

**Target**: All tests pass OR failures understood and documented

**Acceptable Failure Reasons**:
- Test expectation needs updating for ASP.NET Core behavior changes
- Test infrastructure needs refactoring (MvcContrib compatibility)
- RedditSharp 2.0 API changes require test updates

**Unacceptable Failure Reasons**:
- Test infrastructure crash (RavenDB, xUnit, Moq)
- Cannot instantiate controllers (DI issues)
- Cannot load RaccoonBlog.Web assembly

#### Test Coverage Verification

**Validation Criteria**:
- [ ] Test coverage maintained (same areas tested as before)
- [ ] No tests silently skipped or removed
- [ ] Test execution time reasonable

---

### Level 6: Security Validation

**Timing**: After all functionality testing passes

**Scope**: Both projects

#### Package Vulnerability Scan

**Validation Criteria**:
- [ ] Run `dotnet list package --vulnerable`
- [ ] **0 packages with known vulnerabilities**
- [ ] Verify jQuery updated to 3.7.1
- [ ] Verify bootstrap updated to 5.3.8
- [ ] Verify jQuery UI updated to 1.14.1
- [ ] Verify jQuery Validation updated to 1.21.0

**Command**:
```bash
dotnet list package --vulnerable
```

**Expected Output**: No vulnerabilities found.

#### Security Best Practices

**Validation Criteria**:
- [ ] HTTPS enforced (UseHttpsRedirection in Program.cs)
- [ ] HSTS enabled for production (UseHsts)
- [ ] Authentication configured correctly
- [ ] Authorization attributes present on protected actions
- [ ] No sensitive data in appsettings.json (use secrets management)

---

### Level 7: Performance Validation (Optional)

**Timing**: After all other testing passes

**Scope**: RaccoonBlog.Web application

#### Startup Performance

**Validation Criteria**:
- Application startup time acceptable (<5 seconds)
- First request response time acceptable (<2 seconds)

#### Request Performance

**Validation Criteria**:
- Home page load time acceptable
- Post listing page load time acceptable
- Post detail page load time acceptable
- No significant performance regression vs .NET Framework baseline

**Note**: .NET 8 should be faster than .NET Framework 4.7.1 in most scenarios. Performance degradation indicates misconfiguration.

---

### Testing Checklist Summary

#### Phase 1: Atomic Framework and Package Upgrade

**Build Validation**:
- [ ] RaccoonBlog.Web builds with 0 errors
- [ ] RaccoonBlog.IntegrationTests builds with 0 errors
- [ ] Solution builds successfully
- [ ] No package restore errors

**Smoke Tests**:
- [ ] Application starts without exceptions
- [ ] RavenDB embedded initializes
- [ ] Static files serve correctly
- [ ] Home page loads
- [ ] Basic navigation works

**Authentication Tests**:
- [ ] Login page loads
- [ ] Login/logout works
- [ ] User registration works
- [ ] External provider buttons display

**Functionality Tests**:
- [ ] Blog post listing works
- [ ] Post detail view works
- [ ] Post management works (create/edit/delete)
- [ ] Comments work
- [ ] Search works
- [ ] RSS feed works

#### Phase 2: Test Validation

**Integration Tests**:
- [ ] All tests discovered
- [ ] All tests execute
- [ ] RavenDB TestDriver works
- [ ] Mocking framework works
- [ ] Test pass rate acceptable

**Security**:
- [ ] No vulnerable packages
- [ ] Security best practices followed

---

## Success Criteria

### The migration is complete when all criteria in the following categories are met:

---

### 1. Technical Criteria

#### Project Configuration
- [ ] Both projects converted to SDK-style .csproj format
- [ ] Both projects target `net8.0`
- [ ] Project files contain only necessary elements (SDK-style minimalism)
- [ ] Project references correctly defined

#### Package Management
- [ ] All incompatible packages removed or replaced
- [ ] All security-vulnerable packages updated:
  - [ ] bootstrap: 3.3.1 ? 5.3.8 ?
  - [ ] jQuery: 1.11.2 ? 3.7.1 ?
  - [ ] jQuery.UI.Combined: 1.11.2 ? 1.14.1 ?
  - [ ] jQuery.Validation: 1.13.1 ? 1.21.0 ?
- [ ] All recommended package updates applied
- [ ] No package version conflicts
- [ ] Package vulnerability scan shows 0 vulnerabilities: `dotnet list package --vulnerable`

#### Build Success
- [ ] RaccoonBlog.Web builds with 0 errors
- [ ] RaccoonBlog.IntegrationTests builds with 0 errors
- [ ] Solution builds with 0 errors in Release configuration
- [ ] No critical build warnings (CS#### compilation errors)
- [ ] `dotnet restore` succeeds for entire solution
- [ ] `dotnet build` succeeds for entire solution

#### Application Functionality
- [ ] Application starts without exceptions
- [ ] No startup errors in console
- [ ] RavenDB embedded database initializes successfully
- [ ] Kestrel web server starts and listens
- [ ] Home page loads without errors
- [ ] Static files serve correctly (CSS, JavaScript, images)
- [ ] Routing works (all expected routes accessible)

#### Authentication & Authorization
- [ ] Login page loads and functions
- [ ] User can log in with credentials
- [ ] User can log out
- [ ] User registration works
- [ ] Authenticated user identity persists across requests
- [ ] Protected resources require authentication
- [ ] External authentication provider buttons display (Facebook, Google, Twitter, Microsoft)

#### Core Features
- [ ] Blog post listing displays correctly
- [ ] Blog post detail view loads
- [ ] Post creation works (for authenticated users)
- [ ] Post editing works
- [ ] Post deletion works
- [ ] Comments display and can be submitted
- [ ] Search functionality works
- [ ] RSS feed generates valid XML

#### Test Suite
- [ ] All integration tests discovered by test runner
- [ ] xUnit test framework executes tests
- [ ] RavenDB TestDriver initializes without errors
- [ ] Moq mocking framework works correctly
- [ ] All tests execute (no infrastructure failures)
- [ ] Test pass rate: 100% OR failures documented and understood
- [ ] No test coverage regression

---

### 2. Quality Criteria

#### Code Quality
- [ ] No code regressions (existing functionality maintained)
- [ ] Breaking changes handled comprehensively (2,597+ LOC addressed)
- [ ] Legacy APIs updated to modern equivalents
- [ ] Obsolete code removed (Global.asax, BundleConfig, etc.)
- [ ] No System.Web dependencies remain
- [ ] No OWIN dependencies remain

#### Security
- [ ] **All 4 security vulnerabilities patched** (bootstrap, jQuery, jQuery UI, jQuery Validation)
- [ ] Authentication uses ASP.NET Core Identity
- [ ] External authentication configured correctly
- [ ] HTTPS enforced in production (UseHttpsRedirection)
- [ ] HSTS configured for production
- [ ] No sensitive credentials in source code
- [ ] Configuration secrets managed appropriately

#### Maintainability
- [ ] Configuration modernized (web.config ? appsettings.json)
- [ ] Dependency injection used throughout
- [ ] Modern ASP.NET Core patterns followed
- [ ] Code structure follows ASP.NET Core conventions
- [ ] Static files organized in wwwroot

#### Performance
- [ ] Application startup time acceptable (<5 seconds)
- [ ] First request response time acceptable (<2 seconds)
- [ ] No significant performance regression vs .NET Framework baseline
- [ ] Memory usage reasonable

---

### 3. Process Criteria

#### All-At-Once Strategy Execution
- [ ] Both projects migrated simultaneously (atomic operation)
- [ ] No intermediate multi-targeting state
- [ ] Single comprehensive validation phase completed
- [ ] All breaking changes addressed in coordinated manner

#### Migration Plan Adherence
- [ ] All steps in project-by-project migration plans executed
- [ ] All package updates from Package Update Reference applied
- [ ] All breaking changes from Breaking Changes Catalog addressed
- [ ] Dependency order respected (Web before IntegrationTests)

#### Source Control
- [ ] All changes committed to `upgrade-to-NET8-3` branch
- [ ] Commit messages follow conventional format
- [ ] Incremental commits at logical checkpoints (recommended 15 commits)
- [ ] Main branch unaffected until merge
- [ ] Changes ready for code review (if applicable)

#### Documentation
- [ ] Breaking changes documented (in commit messages and/or separate doc)
- [ ] Configuration changes documented
- [ ] Known issues documented (if any)
- [ ] Deployment requirements updated (requires .NET 8 SDK)

---

### 4. All-At-Once Strategy Success Indicators

#### Strategy-Specific Validation
- [ ] Both projects on same target framework (net8.0)
- [ ] No multi-targeting complexity introduced
- [ ] Single build validation pass for entire solution
- [ ] Single test validation pass for entire solution
- [ ] Security vulnerabilities addressed immediately (not deferred)
- [ ] Atomic upgrade completed as planned

#### Efficiency Gains
- [ ] Migration completed in single coordinated operation
- [ ] No intermediate stabilization phases required
- [ ] Test suite validated entire application at once
- [ ] Rollback capability maintained throughout (branch isolation)

---

### 5. Deployment Readiness Criteria

#### Runtime Requirements
- [ ] .NET 8 SDK installed on development machine: `dotnet --version` shows 8.0.x
- [ ] .NET 8 Runtime documented as deployment requirement
- [ ] Hosting requirements documented (Kestrel, IIS with ASP.NET Core Module, etc.)

#### Configuration
- [ ] `appsettings.json` properly configured
- [ ] `appsettings.Production.json` created for production settings (if needed)
- [ ] Connection strings configured correctly
- [ ] External authentication provider credentials configured (via secrets or environment)

#### Database
- [ ] RavenDB embedded database compatible with .NET 8
- [ ] Database schema compatible (if Identity schema changed, migration plan exists)
- [ ] Data integrity maintained

---

### 6. Acceptance Criteria Summary

**The migration is considered complete and successful when:**

? **All Technical Criteria met** (builds, runs, functions correctly)  
? **All Quality Criteria met** (secure, maintainable, performant)  
? **All Process Criteria met** (plan followed, properly documented)  
? **All Strategy Criteria met** (atomic upgrade executed as planned)  
? **Deployment Ready** (requirements documented, configuration complete)

**Final Validation Command Sequence:**

```bash
# 1. Clean build
dotnet clean
dotnet restore
dotnet build --configuration Release

# 2. Security scan
dotnet list package --vulnerable

# 3. Run tests
dotnet test --configuration Release

# 4. Run application
dotnet run --project RaccoonBlog.Web\RaccoonBlog.Web.csproj

# 5. Manual validation
# - Open browser to https://localhost:5001
# - Verify home page loads
# - Test login/logout
# - Test core features
# - Verify no console errors

# 6. Code review (if team process requires)

# 7. Merge to main
git checkout main
git merge upgrade-to-NET8-3 --no-ff
git tag -a v2.0.0-net8 -m "Migrated to .NET 8.0"
git push origin main --tags
```

---

### Sign-Off

**Migration Completed By**: _________________  
**Date**: _________________  
**Validated By**: _________________  
**Date**: _________________  

**Notes**:
- Document any deviations from plan
- Document any known issues or technical debt
- Document any features intentionally deferred

---

**End of Migration Plan**
