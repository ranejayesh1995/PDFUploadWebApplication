using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PDFUploadWebApplication.Core.Entities;
using PDFUploadWebApplication.Core.Repositories;

namespace PDFUploadWebApplication.Core.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentService(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<Document?> GetLatestDocumentAsync(string fileName, Guid userId)
        {
            return await _documentRepository.GetLatestVersionAsync(fileName, userId);
        }
        public async Task<Document?> GetDocumentByNameAsync(Guid userId, string fileName, int? revision)
        {
            var query = _documentRepository.GetDocumentsByUserId(userId,fileName)
                .Where(d => d.FileName == fileName);

            // ✅ If revision is provided, fetch the specific version
            if (revision.HasValue)
            {
                query = query.Where(d => d.Version == revision.Value);
            }
            else
            {
                query = query.OrderByDescending(d => d.Version).Take(1); // Get latest version
            }

            return await query.FirstOrDefaultAsync();
        }


        public async Task<Document?> GetDocumentByRevisionAsync(string fileName, Guid userId, int revision)
        {
            return await _documentRepository.GetSpecificVersionAsync(fileName, userId, revision);
        }

        public async Task<List<Document>> GetUserDocumentsAsync(Guid userId)
        {
            return await _documentRepository.GetAllUserDocumentsAsync(userId);
        }

        public async Task UploadDocumentAsync(IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Invalid file");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var document = new Document
            {
                FileName = file.FileName,
                FileData = memoryStream.ToArray(),
                UploadedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _documentRepository.AddDocumentAsync(document);
        }
        public async Task<List<Document>> GetDocumentsByUserIdAsync(Guid userId, string fileName)
        {
            return await _documentRepository.GetDocumentsByUserId(userId, fileName).ToListAsync();
        }




        public async Task<Document?> GetUserDocumentByNameAsync(Guid userId, string fileName)
        {
            return await _documentRepository.GetDocumentsByUserId(userId, fileName)
                .FirstOrDefaultAsync(); // ✅ Fixes the issue
        }
        public async Task<List<Document>> GetDocumentsByUserIdAsync(Guid userId)
        {
            return await _documentRepository.GetDocumentsByUserId(userId).ToListAsync();
        }

    }
}
