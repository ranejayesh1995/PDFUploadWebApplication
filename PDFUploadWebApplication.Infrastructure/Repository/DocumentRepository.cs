using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PDFUploadWebApplication.Core.Entities;
using PDFUploadWebApplication.Core.Services;
using PDFUploadWebApplication.Infrastructure.Data;

namespace PDFUploadWebApplication.Core.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<Document?> GetLatestVersionAsync(string fileName, Guid userId)
        {
            return await _context.Documents
                .Where(d => d.FileName == fileName && d.UserId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Document?> GetSpecificVersionAsync(string fileName, Guid userId, int revision)
        {
            return await _context.Documents
                .Where(d => d.FileName == fileName && d.UserId == userId)
                .OrderBy(d => d.UploadedAt)
                .Skip(revision)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Document>> GetAllUserDocumentsAsync(Guid userId)
        {
            return await _context.Documents.Where(d => d.UserId == userId).ToListAsync();
        }

        public async Task AddDocumentAsync(Document document)
        {
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
        }
        public IQueryable<Document> GetDocumentsByUserId(Guid userId, string fileName)
        {
            return _context.Documents
                .Where(d => d.UserId == userId ) // ✅ Filter by user ID
                .OrderByDescending(d => d.UploadedAt); // ✅ Sort by newest first
        }


        public Task<Document?> GetUserDocumentByNameAsync(Guid userId, string fileName)
        {
            throw new NotImplementedException();
        }
        public IQueryable<Document> GetDocumentsByUserId(Guid userId)
        {
            return _context.Documents
                .Where(d => d.UserId == userId) // ✅ Filter only by user ID
                .OrderByDescending(d => d.UploadedAt); // ✅ Sort by newest first
        }

    }
}
