
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Karnel_Travel.Data
{
    // Image save karne ka common helper
    public static class ImageUploader
    {
        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private const long MaxBytes = 2 * 1024 * 1024; // 2 MB

        public static async Task<(string? Url, string? Error)> SaveAsync(
            IFormFile? file,
            string webRootPath)
        {
            if (file == null || file.Length == 0)
                return (null, null);

            var ext = Path.GetExtension(file.FileName)
                         .ToLowerInvariant();

            if (!AllowedExtensions.Contains(ext))
                return (null, "Only JPG, PNG or WEBP images are allowed.");

            if (file.Length > MaxBytes)
                return (null, "Image must be smaller than 2 MB.");

            var folder = Path.Combine(webRootPath, "img", "uploads");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return ("/img/uploads/" + fileName, null);
        }
    }
}