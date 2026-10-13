using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Karnel_Travel.Models;
using Karnel_Travel.Data;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin area controller; access limited to the Admin role.
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TouristSpotsController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _context;
        // Provides the web root path for image uploads.
        private readonly IWebHostEnvironment _env;

        // Constructor: dependencies are injected by ASP.NET.
        public TouristSpotsController(
        Karnel_TravelContext context,
        IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Lists records.
        public async Task<IActionResult> Index()
        {
            return View(await _context.TouristSpots.ToListAsync());
        }

        // Shows a single record.
        public async Task<IActionResult> Details(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var touristspot = await _context.TouristSpots
                .FirstOrDefaultAsync(m => m.Id == id);

            // Return 404 when the record does not exist.
            if (touristspot == null)
            {
                return NotFound();
            }

            return View(touristspot);
        }

        // Displays the create form.
        public IActionResult Create()
        {
            return View();
        }

        // Validates input, uploads the image and saves the new record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TouristSpot touristspot, IFormFile? imageFile)
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
                    return View(touristspot);
                }
                // Use the new image only when one was uploaded.
                if (url != null) touristspot.ImagePath = url;

                // Stage the new record.
                _context.Add(touristspot);
                // Persist changes to the database.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(touristspot);
        }

        // Displays the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var touristspot = await _context.TouristSpots.FindAsync(id);

            // Return 404 when the record does not exist.
            if (touristspot == null)
            {
                return NotFound();
            }

            return View(touristspot);
        }

        // Saves the edited record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id,
     [Bind("Id,Name,Category,Location,Description,ImagePath")] TouristSpot touristspot,
     IFormFile? imageFile)
        {
            // Reject a mismatched id.
            if (id != touristspot.Id)
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
                    return View(touristspot);
                }
                // Use the new image only when one was uploaded.
                if (url != null) touristspot.ImagePath = url;

                try
                {
                    _context.Update(touristspot);
                    // Persist changes to the database.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TouristSpotExists(touristspot.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(touristspot);
        }

        // Shows the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var touristspot = await _context.TouristSpots
                .FirstOrDefaultAsync(m => m.Id == id);

            // Return 404 when the record does not exist.
            if (touristspot == null)
            {
                return NotFound();
            }

            return View(touristspot);
        }

        // Deletes the record after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var touristspot = await _context.TouristSpots.FindAsync(id);

            if (touristspot != null)
            {
                // Remove the record.
                _context.TouristSpots.Remove(touristspot);
            }

            // Persist changes to the database.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Helper: checks whether the record exists.
        private bool TouristSpotExists(int? id)
        {
            return _context.TouristSpots.Any(e => e.Id == id);
        }
    }
}
