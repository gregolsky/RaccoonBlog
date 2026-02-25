using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Controllers;

public class ImagesController : Controller
{
    private readonly IDocumentSession _ravenSession;

    public ImagesController(IDocumentSession ravenSession)
    {
        _ravenSession = ravenSession;
    }

    [HttpGet("images/getimage/{id}")]
    [ResponseCache(Duration = 1800)]
    public IActionResult GetImage(string id, [FromQuery] string fileName)
    {
        var docId = "images/" + id;
        
        var attachment = _ravenSession.Advanced.Attachments.Get(docId, fileName);
            
        if (attachment == null)
            return NotFound();
        
        return File(attachment.Stream, attachment.Details.ContentType);
    }
}