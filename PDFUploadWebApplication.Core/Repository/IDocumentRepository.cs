using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDFUploadWebApplication.Core.Entities;

namespace PDFUploadWebApplication.Core.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document?> GetLatestVersionAsync(string fileName, Guid userId);
        Task<Document?> GetSpecificVersionAsync(string fileName, Guid userId, int revision);
        Task<List<Document>> GetAllUserDocumentsAsync(Guid userId);
        Task AddDocumentAsync(Document document);
        Task<Document?> GetUserDocumentByNameAsync(Guid userId, string fileName);
        IQueryable<Document> GetDocumentsByUserId(Guid userId, string fileName);
        IQueryable<Document> GetDocumentsByUserId(Guid userId);

    }
}