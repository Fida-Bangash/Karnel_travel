
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

    public class TravelInfoesController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _context;

        // Constructor: dependencies are injected by ASP.NET.
        public TravelInfoesController(Karnel_TravelContext context)
        {
            _context = context;
        }

        // Lists records.
        public async Task<IActionResult> Index()
        {
            return View(await _context.TravelInfos.ToListAsync());
        }

        // Shows a single record.
        public async Task<IActionResult> Details(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var travelinfo = await _context.TravelInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (travelinfo == null)
            {
                return NotFound();
            }

            return View(travelinfo);
        }

        // Displays the create form.
        public IActionResult Create()
        {
            return View();
        }

        // Validates input and saves the new record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TransportMode,FromCity,ToCity,Fare,Details")] TravelInfo travelinfo)
        {
            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                // Stage the new record.
                _context.Add(travelinfo);
                // Persist changes to the database.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(travelinfo);
        }

        // Displays the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var travelinfo = await _context.TravelInfos.FindAsync(id);
            // Return 404 when the record does not exist.
            if (travelinfo == null)
            {
                return NotFound();
            }
            return View(travelinfo);
        }

        // Saves the edited record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,TransportMode,FromCity,ToCity,Fare,Details")] TravelInfo travelinfo)
        {
            // Reject a mismatched id.
            if (id != travelinfo.Id)
            {
                return NotFound();
            }

            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(travelinfo);
                    // Persist changes to the database.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TravelInfoExists(travelinfo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(travelinfo);
        }

        // Shows the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var travelinfo = await _context.TravelInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (travelinfo == null)
            {
                return NotFound();
            }

            return View(travelinfo);
        }

        // Deletes the record after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var travelinfo = await _context.TravelInfos.FindAsync(id);
            if (travelinfo != null)
            {
                // Remove the record.
                _context.TravelInfos.Remove(travelinfo);
            }

            // Persist changes to the database.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper: checks whether the record exists.
        private bool TravelInfoExists(int? id)
        {
            return _context.TravelInfos.Any(e => e.Id == id);
        }
    }
}