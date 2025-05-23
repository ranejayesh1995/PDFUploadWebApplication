using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFUploadWebApplication.Core.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public byte[]? FileData { get; set; }
        public DateTime UploadedAt { get; set; }

        public Guid UserId { get; set; }  // change from int to Guid to match User.Id

        public User? User { get; set; }

        public int Version { get; set; } // ✅ New column for version tracking
    }


}
