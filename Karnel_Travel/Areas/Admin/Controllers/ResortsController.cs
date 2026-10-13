
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
    public class ResortsController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _context;
        // Provides the web root path for image uploads.
        private readonly IWebHostEnvironment _env;

        // Constructor: dependencies are injected by ASP.NET.
        public ResortsController(
        Karnel_TravelContext context,
        IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Lists records.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Resorts.ToListAsync());
        }

        // Shows a single record.
        public async Task<IActionResult> Details(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var resort = await _context.Resorts
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (resort == null)
            {
                return NotFound();
            }

            return View(resort);
        }

        // Displays the create form.
        public IActionResult Create()
        {
            return View();
        }

        // Validates input, uploads the image and saves the new record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resort resort, IFormFile? imageFile)
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
                    return View(resort);
                }
                // Use the new image only when one was uploaded.
                if (url != null) resort.ImagePath = url;

                // Stage the new record.
                _context.Add(resort);
                // Persist changes to the database.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(resort);
        }

        // Displays the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var resort = await _context.Resorts.FindAsync(id);
            // Return 404 when the record does not exist.
            if (resort == null)
            {
                return NotFound();
            }
            return View(resort);
        }

        // Saves the edited record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id,
    [Bind("Id,Name,Location,Price,Rating,Quantity,IsAvailable,Description,ImagePath")] Resort resort,
    IFormFile? imageFile)
        {
            // Reject a mismatched id.
            if (id != resort.Id)
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
                    return View(resort);
                }
                // Use the new image only when one was uploaded.
                if (url != null) resort.ImagePath = url;

                try
                {
                    _context.Update(resort);
                    // Persist changes to the database.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResortExists(resort.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(resort);
        }

        // Shows the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var resort = await _context.Resorts
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (resort == null)
            {
                return NotFound();
            }

            return View(resort);
        }

        // Deletes the record after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var resort = await _context.Resorts.FindAsync(id);
            if (resort != null)
            {
                // Remove the record.
                _context.Resorts.Remove(resort);
            }

            // Persist changes to the database.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper: checks whether the record exists.
        private bool ResortExists(int? id)
        {
            return _context.Resorts.Any(e => e.Id == id);
        }
    }
}