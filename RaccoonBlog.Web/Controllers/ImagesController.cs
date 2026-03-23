using System.IO;
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
    
    [HttpGet("Images/{**imagePath}")]
    [OutputCache(Duration = 1800)]
    public IActionResult GetImage(string imagePath)
    {
        var fileName = Path.GetFileName(imagePath).ToLowerInvariant();

        if (string.IsNullOrEmpty(fileName))
            return NotFound();
        
        var docId = "images/" + fileName;
        var imageDoc = _ravenSession.Load<PostImage>(docId);

        if (imageDoc == null) return NotFound();
        
        var attachment = _ravenSession.Advanced.Attachments.Get(imageDoc.Id, imageDoc.FileName);
        if (attachment == null) return NotFound();
        
        Response.RegisterForDispose(attachment);
        return File(attachment.Stream, attachment.Details.ContentType);
    }
}