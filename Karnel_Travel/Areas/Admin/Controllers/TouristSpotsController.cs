using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Karnel_Travel.Models;
using Karnel_Travel.Data;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TouristSpotsController : Controller
    {
        private readonly Karnel_TravelContext _context;
        private readonly IWebHostEnvironment _env;

        public TouristSpotsController(
        Karnel_TravelContext context,
        IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/TouristSpots
        public async Task<IActionResult> Index()
        {
            return View(await _context.TouristSpots.ToListAsync());
        }

        // GET: Admin/TouristSpots/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var touristspot = await _context.TouristSpots
                .FirstOrDefaultAsync(m => m.Id == id);

            if (touristspot == null)
            {
                return NotFound();
            }

            return View(touristspot);
        }

        // GET: Admin/TouristSpots/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/TouristSpots/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TouristSpot touristspot, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Image save karo (type aur size check ke saath)
                var (url, error) = await ImageUploader.SaveAsync(imageFile, _env.WebRootPath);
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(touristspot);
                }
                if (url != null) touristspot.ImagePath = url;

                _context.Add(touristspot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(touristspot);
        }

        // GET: Admin/TouristSpots/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var touristspot = await _context.TouristSpots.FindAsync(id);

            if (touristspot == null)
            {
                return NotFound();
            }

            return View(touristspot);
        }

        // POST: Admin/TouristSpots/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id,
     [Bind("Id,Name,Category,Location,Description,ImagePath")] TouristSpot touristspot,
     IFormFile? imageFile)
        {
            if (id != touristspot.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Nayi image di ho to replace karo, warna purani ImagePath hidden field se aa jati hai
                var (url, error) = await ImageUploader.SaveAsync(imageFile, _env.WebRootPath);
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(touristspot);
                }
                if (url != null) touristspot.ImagePath = url;

                try
                {
                    _context.Update(touristspot);
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

        // GET: Admin/TouristSpots/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var touristspot = await _context.TouristSpots
                .FirstOrDefaultAsync(m => m.Id == id);

            if (touristspot == null)
            {
                return NotFound();
            }

            return View(touristspot);
        }

        // POST: Admin/TouristSpots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var touristspot = await _context.TouristSpots.FindAsync(id);

            if (touristspot != null)
            {
                _context.TouristSpots.Remove(touristspot);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TouristSpotExists(int? id)
        {
            return _context.TouristSpots.Any(e => e.Id == id);
        }
    }
}

