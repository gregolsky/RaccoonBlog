using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using RaccoonBlog.Web.Services;
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
    [OutputCache(Duration = 1800)]
    public IActionResult GetImage(string id, [FromQuery] string fileName)
    {
        var docId = "images/" + id;
        
        var imageDoc = _ravenSession.Load<PostImage>(docId);
        if (imageDoc == null) return NotFound();
        
        var realFileName = imageDoc.FileName;
        
        var attachment = _ravenSession.Advanced.Attachments.Get(docId, realFileName);
        if (attachment == null) return NotFound();
        
        Response.RegisterForDispose(attachment);
        return File(attachment.Stream, attachment.Details.ContentType);
    }
}