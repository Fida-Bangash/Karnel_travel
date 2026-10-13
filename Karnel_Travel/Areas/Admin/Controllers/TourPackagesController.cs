
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karnel_Travel.Models;
using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin area controller; access limited to the Admin role.
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TourPackagesController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _context;
        // Provides the web root path for image uploads.
        private readonly IWebHostEnvironment _env;

        // Constructor: dependencies are injected by ASP.NET.
        public TourPackagesController(
        Karnel_TravelContext context,
        IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Lists records.
        public async Task<IActionResult> Index()
        {
            return View(await _context.TourPackages.ToListAsync());
        }

        // Shows a single record.
        public async Task<IActionResult> Details(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var tourpackage = await _context.TourPackages
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (tourpackage == null)
            {
                return NotFound();
            }

            return View(tourpackage);
        }

        // Displays the create form.
        public IActionResult Create()
        {
            return View();
        }

        // Validates input, uploads the image and saves the new record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TourPackage tourpackage, IFormFile? imageFile)
        {
            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                // Validate and save the uploaded image.
                var (url, error) = await ImageUploader.SaveAsync(imageFile, _env.WebRootPath);
                // Redisplay the form with the upload error.
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(tourpackage);
                }
                // Use the new image only when one was uploaded.
                if (url != null) tourpackage.ImagePath = url;

                // Stage the new record.
                _context.Add(tourpackage);
                // Persist changes to the database.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tourpackage);
        }

        // Displays the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var tourpackage = await _context.TourPackages.FindAsync(id);
            // Return 404 when the record does not exist.
            if (tourpackage == null)
            {
                return NotFound();
            }
            return View(tourpackage);
        }

        // Saves the edited record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id,
    [Bind("Id,Title,Details,Price,DiscountPercent,ValidTill,ImagePath")] TourPackage tourpackage,
    IFormFile? imageFile)
        {
            // Reject a mismatched id.
            if (id != tourpackage.Id)
            {
                return NotFound();
            }

            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                // Validate and save the uploaded image.
                var (url, error) = await ImageUploader.SaveAsync(imageFile, _env.WebRootPath);
                // Redisplay the form with the upload error.
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(tourpackage);
                }
                // Use the new image only when one was uploaded.
                if (url != null) tourpackage.ImagePath = url;

                try
                {
                    _context.Update(tourpackage);
                    // Persist changes to the database.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourPackageExists(tourpackage.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tourpackage);
        }

        // Shows the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var tourpackage = await _context.TourPackages
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (tourpackage == null)
            {
                return NotFound();
            }

            return View(tourpackage);
        }

        // Deletes the record after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var tourpackage = await _context.TourPackages.FindAsync(id);
            if (tourpackage != null)
            {
                // Remove the record.
                _context.TourPackages.Remove(tourpackage);
            }

            // Persist changes to the database.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper: checks whether the record exists.
        private bool TourPackageExists(int? id)
        {
            return _context.TourPackages.Any(e => e.Id == id);
        }
    }
}
