# TempData Serialization Fix for CommentInput

## Problem

When posting a comment, the application threw an exception:

```
InvalidOperationException: The 'Microsoft.AspNetCore.Mvc.NewtonsoftJson.BsonTempDataSerializer' 
cannot serialize an object of type 'RaccoonBlog.Web.ViewModels.CommentInput'.
```

This occurred in the `PostDetailsController.PostingCommentSucceeded()` method when trying to store the `CommentInput` object in TempData:

```csharp
TempData["new-comment"] = input;
```

## Root Cause

By default, when using `.AddNewtonsoftJson()`, ASP.NET Core configures TempData to use the `BsonTempDataSerializer` which stores data in cookies. BSON serialization has stricter requirements and may fail for certain object types, even simple POCOs.

## Solutions Implemented

### Solution 1: Add [Serializable] Attribute to CommentInput

**File:** `RaccoonBlog.Web/ViewModels/CommentInput.cs`

```csharp
[Serializable]
public class CommentInput
{
    // ... properties
}
```

This marks the class as serializable for BSON serialization.

### Solution 2: Use Session-Based TempData Provider (Recommended)

**File:** `RaccoonBlog.Web/Program.cs`

#### Added Session Configuration:

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

#### Changed TempData Provider:

```csharp
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new GuidBinderProvider());
})
.AddNewtonsoftJson()
.AddSessionStateTempDataProvider(); // Use session-based TempData
```

#### Added Session Middleware:

```csharp
app.UseSession(); // Must be before UseAuthentication and UseAuthorization
```

## Why Session-Based TempData is Better

### Advantages:

1. **No Serialization Issues** - Session-based TempData uses standard .NET serialization, which works with all objects
2. **Larger Data Capacity** - Not limited by cookie size constraints (4KB limit)
3. **Better Performance** - No need to serialize/deserialize on every request
4. **More Secure** - Data stored server-side, not exposed in cookies

### Disadvantages:

1. **Server Memory** - Uses server memory instead of client cookies
2. **Scalability** - Requires session affinity in load-balanced scenarios (or distributed session state)

## Cookie-Based vs Session-Based TempData

| Aspect | Cookie-Based (BSON) | Session-Based |
|--------|---------------------|---------------|
| Storage | Client cookie | Server memory/cache |
| Size Limit | 4KB | Much larger |
| Serialization | BSON (strict) | Standard .NET |
| Performance | Slower (serialize each request) | Faster |
| Scalability | Stateless | Requires sticky sessions or distributed cache |
| Security | Data sent to client | Data stays on server |

## Alternative Solutions (Not Implemented)

### Option 1: Custom JSON TempData Serializer

```csharp
builder.Services.Configure<MvcOptions>(options =>
{
    options.TempDataSerializer = new JsonTempDataSerializer();
});
```

This would allow cookie-based TempData with JSON serialization instead of BSON.

### Option 2: Avoid TempData for Complex Objects

Instead of:
```csharp
TempData["new-comment"] = input;
```

Use query string parameters:
```csharp
return RedirectToAction("Details", new { 
    id, 
    slug, 
    name = input.Name,
    email = input.Email,
    // ... other properties
});
```

Or use in-memory cache:
```csharp
var cacheKey = Guid.NewGuid().ToString();
_memoryCache.Set(cacheKey, input, TimeSpan.FromMinutes(5));
TempData["comment-cache-key"] = cacheKey;
```

## Testing Checklist

After applying this fix, verify:

- [ ] Comment submission works without errors
- [ ] Comment appears in the post after submission
- [ ] Success message displays correctly
- [ ] Ajax comment submission works (if applicable)
- [ ] Comment validation errors display correctly
- [ ] Session cookie is created (check browser dev tools)
- [ ] Application works in multiple browsers
- [ ] Load balancer configuration updated if needed (sticky sessions)

## Important Notes

1. **Session Middleware Order**: `UseSession()` must be called BEFORE `UseAuthentication()` and `UseAuthorization()`

2. **Distributed Session State**: For production environments with multiple servers, consider using distributed session state:
   ```csharp
   builder.Services.AddStackExchangeRedisCache(options =>
   {
       options.Configuration = "localhost:6379";
   });
   builder.Services.AddSession(options =>
   {
       // ... options
   });
   ```

3. **Session Security**: Sessions are already configured with:
   - `HttpOnly = true` - Prevents JavaScript access
   - `IsEssential = true` - Works even when user declines non-essential cookies

## Files Modified

1. ? **CommentInput.cs** - Added `[Serializable]` attribute
2. ? **Program.cs** - Configured session-based TempData provider and added session middleware

## Migration Impact

- **Breaking Changes**: None
- **Configuration Changes**: Requires session configuration
- **Performance Impact**: Minimal (session overhead is negligible)
- **Scalability Impact**: May require sticky sessions or distributed cache in load-balanced scenarios

## Rollback Procedure

To revert to cookie-based TempData:

1. Remove `.AddSessionStateTempDataProvider()` from Program.cs
2. Remove session configuration and middleware
3. The default cookie-based provider will be used

Note: With the `[Serializable]` attribute added, cookie-based TempData should also work now.
