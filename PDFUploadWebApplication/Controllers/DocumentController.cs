using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PDFUploadWebApplication.Core.Entities;
using PDFUploadWebApplication.Core.Services;

namespace PDFUploadWebApplication.Controllers
{

    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // 🔹 Render Upload Form

        //public ActionResult Upload()
        //{
            

        //    return View();
        //}
        
        public async Task<IActionResult> Upload()
        {
            var token = Request.Cookies["JwtToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract user ID (stored as ClaimTypes.NameIdentifier)
            var userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            

            if (string.IsNullOrEmpty(userName) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var documents = await _documentService.GetUserDocumentsAsync(userId);
            return View(documents); // ✅ Pass files to the view
        }



        

        //[HttpPost]
        //public async Task<ActionResult> UploadFile(IFormFile file)
        //{
        //    var token = Request.Cookies["JwtToken"];

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    var handler = new JwtSecurityTokenHandler();
        //    var jwtToken = handler.ReadJwtToken(token);

        //    var userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //    var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        //    if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userName))
        //    {
        //        return Unauthorized("User is not authenticated.");
        //    }

        //    if (!Guid.TryParse(userIdString, out Guid userId))
        //    {
        //        return BadRequest("Invalid user ID format.");
        //    }

        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("Please select a valid file.");
        //    }

        //    using var memoryStream = new MemoryStream();
        //    await file.CopyToAsync(memoryStream);

        //    // ✅ Check if a file with the same name already exists for this user
        //    var latestDocument = await _documentService.GetLatestDocumentAsync(file.FileName, userId);
        //    int newVersion = latestDocument != null ? latestDocument.Version + 1 : 0;

        //    var document = new Document
        //    {
        //        FileName = file.FileName,
        //        FileData = memoryStream.ToArray(),
        //        UploadedAt = DateTime.UtcNow,
        //        UserId = userId,
        //        Version = newVersion
        //    };

        //    await _documentService.UploadDocumentAsync(file, userId);
        //    return Ok($"File uploaded successfully by {userName} as version {newVersion}!");
        //}

        [HttpPost]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            try
            {
                var token = Request.Cookies["JwtToken"];

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Auth");
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userName))
                {
                    return Unauthorized("User is not authenticated.");
                }

                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return BadRequest("Invalid user ID format.");
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("Please select a valid file.");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                // ✅ Check if a file with the same name already exists for this user
                var latestDocument = await _documentService.GetLatestDocumentAsync(file.FileName, userId);
                int newVersion = latestDocument != null ? latestDocument.Version + 1 : 0;

                var document = new Document
                {
                    FileName = file.FileName,
                    FileData = memoryStream.ToArray(),
                    UploadedAt = DateTime.UtcNow,
                    UserId = userId,
                    Version = newVersion
                };

                await _documentService.UploadDocumentAsync(file, userId); // ✅ Fix incorrect parameter passing
                return Ok($"File uploaded successfully by {userName} as version {newVersion}!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadFile: {ex.Message}");
                return StatusCode(500, "An error occurred while uploading the file. Please try again later.");
            }
        }



        // 🔹 Retrieve Latest or Specific Version via Service Layer
        [HttpGet]
        public async Task<ActionResult> GetFile(string fileName, int? revision)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                ViewBag.Message = "File name is required.";
                return View("Upload");
            }

            var token = Request.Cookies["JwtToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract user ID (stored as ClaimTypes.NameIdentifier)
            var userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userName))
            {
                return Unauthorized("User is not authenticated.");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                ViewBag.Message = "Invalid user ID format.";
                return View("Upload");
            }

            var document = revision.HasValue
                ? await _documentService.GetDocumentByRevisionAsync(fileName, userId, revision.Value)
                : await _documentService.GetLatestDocumentAsync(fileName, userId);

            if (document == null || document.FileData == null)
            {
                ViewBag.Message = "File not found.";
                return View("Upload"); // Redirect to upload page
            }

            return File(document.FileData, "application/octet-stream", document.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> ViewFile(string fileName)
        {
            var token = Request.Cookies["JwtToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userName) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var document = await _documentService.GetUserDocumentByNameAsync(userId, fileName);
            if (document == null)
            {
                return NotFound("File not found.");
            }

            return File(document.FileData, "application/pdf", document.FileName, enableRangeProcessing: true);
        }




    }
}
