
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
    public class RestaurantsController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _context;
        // Provides the web root path for image uploads.
        private readonly IWebHostEnvironment _env;

        // Constructor: dependencies are injected by ASP.NET.
        public RestaurantsController(
         Karnel_TravelContext context,
        IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Lists records.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Restaurants.ToListAsync());
        }

        // Shows a single record.
        public async Task<IActionResult> Details(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // Displays the create form.
        public IActionResult Create()
        {
            return View();
        }

        // Validates input, uploads the image and saves the new record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Restaurant restaurant, IFormFile? imageFile)
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
                    return View(restaurant);
                }
                // Use the new image only when one was uploaded.
                if (url != null) restaurant.ImagePath = url;

                // Stage the new record.
                _context.Add(restaurant);
                // Persist changes to the database.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // Displays the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants.FindAsync(id);
            // Return 404 when the record does not exist.
            if (restaurant == null)
            {
                return NotFound();
            }
            return View(restaurant);
        }

        // Saves the edited record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id,
    [Bind("Id,Name,Location,Cuisine,Price,Rating,Quantity,IsAvailable,Description,ImagePath")] Restaurant restaurant,
    IFormFile? imageFile)
        {
            // Reject a mismatched id.
            if (id != restaurant.Id)
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
                    return View(restaurant);
                }
                // Use the new image only when one was uploaded.
                if (url != null) restaurant.ImagePath = url;

                try
                {
                    _context.Update(restaurant);
                    // Persist changes to the database.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantExists(restaurant.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // Shows the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // Deletes the record after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant != null)
            {
                // Remove the record.
                _context.Restaurants.Remove(restaurant);
            }

            // Persist changes to the database.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper: checks whether the record exists.
        private bool RestaurantExists(int? id)
        {
            return _context.Restaurants.Any(e => e.Id == id);
        }
    }
}