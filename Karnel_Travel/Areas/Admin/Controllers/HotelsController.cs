
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karnel_Travel.Models;
using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin area controller; access limited to the Admin role.
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HotelsController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _context;
        // Provides the web root path for image uploads.
        private readonly IWebHostEnvironment _env;

          // Constructor: dependencies are injected by ASP.NET.
          public HotelsController(
         Karnel_TravelContext context,
         IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Lists records.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Hotels.ToListAsync());
        }

        // Shows a single record.
        public async Task<IActionResult> Details(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // Displays the create form.
        public IActionResult Create()
        {
            return View();
        }

        // Validates input, uploads the image and saves the new record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hotel hotel, IFormFile? imageFile)
        {
            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                // Validate and save the uploaded image.
                var (url, error) = await ImageUploader.SaveAsync(
                    imageFile, _env.WebRootPath);

                // Redisplay the form with the upload error.
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(hotel);
                }

                // Use the new image only when one was uploaded.
                if (url != null)
                    hotel.ImagePath = url;

                // Stage the new record.
                _context.Add(hotel);
                // Persist changes to the database.
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(hotel);
        }

        // Displays the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels.FindAsync(id);
            // Return 404 when the record does not exist.
            if (hotel == null)
            {
                return NotFound();
            }
            return View(hotel);
        }

        // Saves the edited record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Hotel hotel, IFormFile? imageFile)
        {
            // Reject a missing or mismatched id.
            if (id == null || id != hotel.Id)
                return NotFound();

            // Load the existing record to update.
            var existingHotel = await _context.Hotels.FindAsync(id);

            // Return 404 when the record does not exist.
            if (existingHotel == null)
                return NotFound();

            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                // Validate and save the uploaded image.
                var (url, error) = await ImageUploader.SaveAsync(
                    imageFile, _env.WebRootPath);

                // Redisplay the form with the upload error.
                if (error != null)
                {
                    ModelState.AddModelError("imageFile", error);
                    return View(hotel);
                }

                // Copy the edited values onto the tracked entity.
                existingHotel.Name = hotel.Name;
                existingHotel.Location = hotel.Location;
                existingHotel.Price = hotel.Price;
                existingHotel.Rating = hotel.Rating;
                existingHotel.Quantity = hotel.Quantity;
                existingHotel.IsAvailable = hotel.IsAvailable;
                existingHotel.Description = hotel.Description;

                // Use the new image only when one was uploaded.
                if (url != null)
                {
                    // Copy the edited values onto the tracked entity.
                    existingHotel.ImagePath = url;
                }

                // Persist changes to the database.
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(hotel);
        }

        // Shows the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // Deletes the record after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                // Remove the record.
                _context.Hotels.Remove(hotel);
            }

            // Persist changes to the database.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper: checks whether the record exists.
        private bool HotelExists(int? id)
        {
            return _context.Hotels.Any(e => e.Id == id);
        }
    }
}