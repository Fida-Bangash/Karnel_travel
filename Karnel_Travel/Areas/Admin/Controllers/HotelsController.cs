
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karnel_Travel.Models;
using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HotelsController : Controller
    {
        private readonly Karnel_TravelContext _context;
        private readonly IWebHostEnvironment _env;

          public HotelsController(
         Karnel_TravelContext context,
         IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: HOTELS
        public async Task<IActionResult> Index()
        {
            return View(await _context.Hotels.ToListAsync());
        }

        // GET: HOTELS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // GET: HOTELS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HOTELS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hotel hotel, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var (url, error) = await ImageUploader.SaveAsync(
                    imageFile, _env.WebRootPath);

                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(hotel);
                }

                if (url != null)
                    hotel.ImagePath = url;

                _context.Add(hotel);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(hotel);
        }

        // GET: HOTELS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }
            return View(hotel);
        }

        // POST: HOTELS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Hotel hotel, IFormFile? imageFile)
        {
            if (id == null || id != hotel.Id)
                return NotFound();

            // Database se existing hotel load karo
            var existingHotel = await _context.Hotels.FindAsync(id);

            if (existingHotel == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                // ImageUploader se nayi image validate aur save karo
                var (url, error) = await ImageUploader.SaveAsync(
                    imageFile, _env.WebRootPath);

                if (error != null)
                {
                    ModelState.AddModelError("imageFile", error);
                    return View(hotel);
                }

                // Existing hotel ki fields update karo
                existingHotel.Name = hotel.Name;
                existingHotel.Location = hotel.Location;
                existingHotel.Price = hotel.Price;
                existingHotel.Rating = hotel.Rating;
                existingHotel.Quantity = hotel.Quantity;
                existingHotel.IsAvailable = hotel.IsAvailable;
                existingHotel.Description = hotel.Description;

                // Sirf nayi image select hui ho to ImagePath change karo
                if (url != null)
                {
                    existingHotel.ImagePath = url;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(hotel);
        }

        // GET: HOTELS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // POST: HOTELS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                _context.Hotels.Remove(hotel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HotelExists(int? id)
        {
            return _context.Hotels.Any(e => e.Id == id);
        }
    }
}