using System;
using System.IO;
using System.Security.Cryptography;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Services;

public class PostImage
{
    public string Id { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public string FileName { get; set; }
}

public class MediaService
{
    private readonly IDocumentSession _session;
    
    public MediaService(IDocumentSession session)
    {
        _session = session;
    }
    
    public string SaveImage(Stream stream, string fileName, string contentType)
    {   
        var safeFileName = Path.GetFileName(fileName).ToLowerInvariant();
        var imageDocId = "images/" + safeFileName;
        
        var existingDoc = _session.Load<PostImage>(imageDocId);
        
        if (existingDoc == null)
        {
            var imageDoc = new PostImage 
            { 
                Id = imageDocId, 
                UploadedAt = DateTimeOffset.Now, 
                FileName = safeFileName 
            };
                
            _session.Store(imageDoc);
            
            stream.Position = 0;
            _session.Advanced.Attachments.Store(imageDocId, safeFileName, stream, contentType);
                
            _session.SaveChanges();
        }

        return safeFileName;
    }
}