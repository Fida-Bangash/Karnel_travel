
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karnel_Travel.Models;
using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TourPackagesController : Controller
    {
        private readonly Karnel_TravelContext _context;
        private readonly IWebHostEnvironment _env;

        public TourPackagesController(
        Karnel_TravelContext context,
        IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: TOURPACKAGES
        public async Task<IActionResult> Index()
        {
            return View(await _context.TourPackages.ToListAsync());
        }

        // GET: TOURPACKAGES/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tourpackage = await _context.TourPackages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tourpackage == null)
            {
                return NotFound();
            }

            return View(tourpackage);
        }

        // GET: TOURPACKAGES/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TOURPACKAGES/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TourPackage tourpackage, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Image save karo (type aur size check ke saath)
                var (url, error) = await ImageUploader.SaveAsync(imageFile, _env.WebRootPath);
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(tourpackage);
                }
                if (url != null) tourpackage.ImagePath = url;

                _context.Add(tourpackage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tourpackage);
        }

        // GET: TOURPACKAGES/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tourpackage = await _context.TourPackages.FindAsync(id);
            if (tourpackage == null)
            {
                return NotFound();
            }
            return View(tourpackage);
        }

        // POST: TOURPACKAGES/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id,
    [Bind("Id,Title,Details,Price,DiscountPercent,ValidTill,ImagePath")] TourPackage tourpackage,
    IFormFile? imageFile)
        {
            if (id != tourpackage.Id)
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
                    return View(tourpackage);
                }
                if (url != null) tourpackage.ImagePath = url;

                try
                {
                    _context.Update(tourpackage);
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

        // GET: TOURPACKAGES/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tourpackage = await _context.TourPackages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tourpackage == null)
            {
                return NotFound();
            }

            return View(tourpackage);
        }

        // POST: TOURPACKAGES/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var tourpackage = await _context.TourPackages.FindAsync(id);
            if (tourpackage != null)
            {
                _context.TourPackages.Remove(tourpackage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TourPackageExists(int? id)
        {
            return _context.TourPackages.Any(e => e.Id == id);
        }
    }
}
