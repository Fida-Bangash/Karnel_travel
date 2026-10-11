
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karnel_Travel.Models;
using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ResortsController : Controller
    {
        private readonly Karnel_TravelContext _context;
        private readonly IWebHostEnvironment _env;

        public ResortsController(
        Karnel_TravelContext context,
        IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: RESORTS
        public async Task<IActionResult> Index()
        {
            return View(await _context.Resorts.ToListAsync());
        }

        // GET: RESORTS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resort = await _context.Resorts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (resort == null)
            {
                return NotFound();
            }

            return View(resort);
        }

        // GET: RESORTS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RESORTS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resort resort, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Image save karo (type aur size check ke saath)
                var (url, error) = await ImageUploader.SaveAsync(imageFile, _env.WebRootPath);
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(resort);
                }
                if (url != null) resort.ImagePath = url;

                _context.Add(resort);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(resort);
        }

        // GET: RESORTS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resort = await _context.Resorts.FindAsync(id);
            if (resort == null)
            {
                return NotFound();
            }
            return View(resort);
        }

        // POST: RESORTS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id,
    [Bind("Id,Name,Location,Price,Rating,Quantity,IsAvailable,Description,ImagePath")] Resort resort,
    IFormFile? imageFile)
        {
            if (id != resort.Id)
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
                    return View(resort);
                }
                if (url != null) resort.ImagePath = url;

                try
                {
                    _context.Update(resort);
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

        // GET: RESORTS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resort = await _context.Resorts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (resort == null)
            {
                return NotFound();
            }

            return View(resort);
        }

        // POST: RESORTS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var resort = await _context.Resorts.FindAsync(id);
            if (resort != null)
            {
                _context.Resorts.Remove(resort);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ResortExists(int? id)
        {
            return _context.Resorts.Any(e => e.Id == id);
        }
    }
}