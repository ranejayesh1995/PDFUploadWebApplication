using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PDFUploadWebApplication.Core.Entities;

namespace PDFUploadWebApplication.Core.Services
{
    public interface IDocumentService
    {
        Task<Document?> GetLatestDocumentAsync(string fileName, Guid userId);
        Task<Document?> GetDocumentByRevisionAsync(string fileName, Guid userId, int revision);
        Task<List<Document>> GetUserDocumentsAsync(Guid userId);
        Task UploadDocumentAsync(IFormFile file, Guid userId);
        Task<Document?> GetUserDocumentByNameAsync(Guid userId, string fileName);
        Task<Document?> GetDocumentByNameAsync(Guid userId, string fileName, int? revision);
        Task<List<Document>> GetDocumentsByUserIdAsync(Guid userId);
    }

}
