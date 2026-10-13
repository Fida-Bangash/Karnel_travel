
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Karnel_Travel.Data
{
    // Shared helper for validating and saving uploaded images.
    public static class ImageUploader
    {
        // Allowed image file types.
        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        // Maximum upload size (2 MB).
        private const long MaxBytes = 2 * 1024 * 1024;

        // Validates and saves an image under wwwroot/img/uploads; returns its URL or an error.
        public static async Task<(string? Url, string? Error)> SaveAsync(
            IFormFile? file,
            string webRootPath)
        {
            // No file selected: nothing to save, not an error.
            if (file == null || file.Length == 0)
                return (null, null);

            // Read the file extension.
            var ext = Path.GetExtension(file.FileName)
                         .ToLowerInvariant();

            // Reject unsupported file types.
            if (!AllowedExtensions.Contains(ext))
                return (null, "Only JPG, PNG or WEBP images are allowed.");

            // Reject files over the size limit.
            if (file.Length > MaxBytes)
                return (null, "Image must be smaller than 2 MB.");

            // Make sure the upload folder exists.
            var folder = Path.Combine(webRootPath, "img", "uploads");
            Directory.CreateDirectory(folder);

            // Use a random file name to avoid collisions and unsafe names.
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(folder, fileName);

            // Write the file to disk.
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the public URL.
            return ("/img/uploads/" + fileName, null);
        }
    }
}