using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Cdsqg.Application.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveEvidenceFileAsync(IFormFile file);
        Task<string> SaveDocumentFileAsync(IFormFile file);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        private const long MaxSizeBytes = 10 * 1024 * 1024; // 10 MB limit

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveEvidenceFileAsync(IFormFile file)
        {
            return await SaveFileInternalAsync(file, "evidence");
        }

        public async Task<string> SaveDocumentFileAsync(IFormFile file)
        {
            return await SaveFileInternalAsync(file, "documents");
        }

        private async Task<string> SaveFileInternalAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            if (file.Length > MaxSizeBytes)
            {
                throw new InvalidOperationException("Dung lượng file vượt quá giới hạn tối đa 10MB.");
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Định dạng file '{extension}' không hợp lệ. Chỉ hỗ trợ .pdf, .doc, .docx, .xls, .xlsx");
            }

            string webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            string uploadsFolder = Path.Combine(webRoot, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Clean original filename (remove special chars/spaces)
            string safeBaseName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "_")
                .Replace("#", "")
                .Replace("?", "");
            if (safeBaseName.Length > 30) safeBaseName = safeBaseName.Substring(0, 30);

            string uniqueFileName = $"{Guid.NewGuid()}_{safeBaseName}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folderName}/{uniqueFileName}";
        }
    }
}
