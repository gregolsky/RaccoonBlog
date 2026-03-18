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

    public (string FileHash, string FileName) SaveImage(Stream stream, string fileName, string contentType)
    {
        string fileHash;
        
        stream.Position = 0;
        using (var sha1 = SHA1.Create())
        {
            var hashBytes = sha1.ComputeHash(stream);
            fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        var imageDocId = "images/" + fileHash;
        
        var existingDoc = _session.Load<PostImage>(imageDocId);
        if (existingDoc == null)
        {
            var imageDoc = new PostImage 
            { 
                Id = imageDocId, 
                UploadedAt = DateTimeOffset.Now, 
                FileName = fileName 
            };
                
            _session.Store(imageDoc);
            
            stream.Position = 0;
            _session.Advanced.Attachments.Store(imageDocId, fileName, stream, contentType);
                
            _session.SaveChanges();
        }

        return (fileHash, fileName);
    }
}