# RaccoonBlog .NET 8.0 Upgrade Tasks

## Overview

This document tracks the execution of the RaccoonBlog solution upgrade from .NET Framework 4.7.1 to .NET 8.0. Both projects (RaccoonBlog.Web and RaccoonBlog.IntegrationTests) will be upgraded simultaneously in a single atomic operation using modern ASP.NET Core 8.0 patterns, followed by comprehensive testing and validation.

**Progress**: 1/3 tasks complete (33%) ![0%](https://progress-bar.xyz/33)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-01-08 13:34)*
**References**: Plan §Prerequisites

- [✓] (1) Verify .NET 8 SDK installed on development machine per Plan §Prerequisites
- [✓] (2) .NET 8 SDK version 8.0.x confirmed via `dotnet --version` (**Verify**)

---

### [▶] TASK-002: Atomic framework and dependency upgrade with compilation fixes
**References**: Plan §Implementation Timeline Phase 1, Plan §Project-by-Project Migration Plans, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [▶] (1) Convert RaccoonBlog.Web project to SDK-style .csproj format per Plan §RaccoonBlog.Web §Convert Project to SDK-Style
- [ ] (2) Convert RaccoonBlog.IntegrationTests project to SDK-style .csproj format per Plan §RaccoonBlog.IntegrationTests §Convert Project to SDK-Style
- [ ] (3) Update TargetFramework to net8.0 in both projects
- [ ] (4) Both projects configured with net8.0 target framework (**Verify**)
- [ ] (5) Remove incompatible packages from RaccoonBlog.Web per Plan §Package Update Reference (12 framework-included packages, 34 incompatible packages)
- [ ] (6) Replace incompatible packages in RaccoonBlog.Web per Plan §Package Update Reference (key replacements: Antlr3→Antlr4, ASP.NET Identity→ASP.NET Core Identity, NLog→NLog.Web.AspNetCore, RhinoMocks→Moq)
- [ ] (7) Update security-vulnerable packages in RaccoonBlog.Web per Plan §Package Update Reference (bootstrap 3.3.1→5.3.8, jQuery 1.11.2→3.7.1, jQuery.UI.Combined 1.11.2→1.14.1, jQuery.Validation 1.13.1→1.21.0)
- [ ] (8) Add new ASP.NET Core packages to RaccoonBlog.Web per Plan §Package Update Reference (Authentication.Facebook, Authentication.Google, Authentication.MicrosoftAccount, Authentication.Twitter, Identity.EntityFrameworkCore, Configuration.Json, Logging.Console)
- [ ] (9) Update RaccoonBlog.IntegrationTests packages per Plan §RaccoonBlog.IntegrationTests §Update Package References (remove Microsoft.AspNet.Mvc, replace RhinoMocks with Moq, update RedditSharp 1.1.14→2.0.0)
- [ ] (10) All package references updated across both projects (**Verify**)
- [ ] (11) Run `dotnet restore` for entire solution
- [ ] (12) All dependencies restored successfully (**Verify**)
- [✓] (13) Create Program.cs in RaccoonBlog.Web per Plan §Breaking Changes Catalog §Application Initialization (replace Global.asax, configure services, middleware pipeline, routing)
- [✓] (14) Create appsettings.json in RaccoonBlog.Web per Plan §Breaking Changes Catalog §Configuration System (migrate web.config appSettings and connectionStrings)
- [ ] (15) Create wwwroot folder structure in RaccoonBlog.Web and move static files per Plan §Breaking Changes Catalog §Static Files & wwwroot (Content→wwwroot/css, Scripts→wwwroot/js, Images→wwwroot/images, fonts→wwwroot/fonts)
- [▶] (16) Update all controller files in RaccoonBlog.Web per Plan §Breaking Changes Catalog §ASP.NET MVC→ASP.NET Core MVC (97 files: change base class, update namespaces, update action result types, update HTTP attributes, update properties)
- [▶] (17) Update all view files in RaccoonBlog.Web per Plan §Breaking Changes Catalog §Views (update @using directives, update HTML helpers, remove bundling references, update Ajax helpers)
- [ ] (18) Update authentication configuration in RaccoonBlog.Web per Plan §Breaking Changes Catalog §Authentication & Authorization (migrate OWIN to ASP.NET Core middleware, update ASP.NET Identity 2.x to ASP.NET Core Identity, configure external providers)
- [ ] (19) Update RavenDB document store registration in RaccoonBlog.Web per Plan §Breaking Changes Catalog §RavenDB Integration (register as singleton in DI)
- [ ] (20) Update NLog configuration in RaccoonBlog.Web per Plan §Breaking Changes Catalog §Logging (create nlog.config, configure in Program.cs)
- [✓] (21) Remove obsolete files from RaccoonBlog.Web per Plan §Code Modifications Priority 3 (Global.asax, BundleConfig.cs, AttributeRouting configurations, OWIN Startup.cs)
- [ ] (22) Update test class namespaces in RaccoonBlog.IntegrationTests per Plan §RaccoonBlog.IntegrationTests §Code Modifications (8 files: change System.Web.Mvc→Microsoft.AspNetCore.Mvc, System.Web→Microsoft.AspNetCore.Http)
- [ ] (23) Migrate RhinoMocks to Moq in RaccoonBlog.IntegrationTests per Plan §RaccoonBlog.IntegrationTests §Breaking Changes (replace mock creation, stub/setup patterns, assertion patterns)
- [ ] (24) Update RedditSharp usage in RaccoonBlog.IntegrationTests per Plan §RaccoonBlog.IntegrationTests §Breaking Changes (update to RedditSharp 2.0 API patterns)
- [ ] (25) Update controller test setup in RaccoonBlog.IntegrationTests per Plan §RaccoonBlog.IntegrationTests §Code Modifications (update instantiation, HttpContext mocking, action result assertions)
- [✓] (26) Build RaccoonBlog.Web project
- [▶] (27) Fix all remaining compilation errors in RaccoonBlog.Web using Plan §Breaking Changes Catalog as reference
- [ ] (28) Build RaccoonBlog.IntegrationTests project
- [ ] (29) Fix all remaining compilation errors in RaccoonBlog.IntegrationTests using Plan §RaccoonBlog.IntegrationTests §Breaking Changes as reference
- [ ] (30) Build entire solution in Release configuration
- [ ] (31) Solution builds with 0 errors (**Verify**)
- [ ] (32) Solution builds with 0 critical warnings (**Verify**)
- [ ] (33) Commit changes with message: "TASK-002: Complete atomic upgrade to .NET 8.0 with ASP.NET Core migration"

---

### [ ] TASK-003: Execute test suite and validate upgrade
**References**: Plan §Implementation Timeline Phase 2, Plan §Testing & Validation Strategy

- [ ] (1) Run all integration tests in RaccoonBlog.IntegrationTests project
- [ ] (2) Fix test failures related to ASP.NET Core test infrastructure, RavenDB TestDriver updates, and MVC test helper compatibility per Plan §RaccoonBlog.IntegrationTests §Testing Strategy
- [ ] (3) Re-run integration tests after fixes
- [ ] (4) All integration tests pass with 0 failures (**Verify**)
- [ ] (5) Run `dotnet list package --vulnerable` to verify no security vulnerabilities
- [ ] (6) Package vulnerability scan shows 0 vulnerabilities (**Verify**)
- [ ] (7) Verify bootstrap updated to 5.3.8, jQuery to 3.7.1, jQuery.UI.Combined to 1.14.1, jQuery.Validation to 1.21.0 per Plan §Success Criteria
- [ ] (8) All security-vulnerable packages confirmed updated (**Verify**)
- [ ] (9) Commit test fixes with message: "TASK-003: Complete testing and validation for .NET 8.0 upgrade"

---

















