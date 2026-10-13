
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
    public class FeedbacksController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _context;

        // Constructor: dependencies are injected by ASP.NET.
        public FeedbacksController(Karnel_TravelContext context)
        {
            _context = context;
        }

        // Lists records.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Feedbacks.AsNoTracking().ToListAsync());
        }

        // Shows a single record.
        public async Task<IActionResult> Details(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // Displays the create form.
        public IActionResult Create()
        {
            return View();
        }

        // Validates input and saves the new record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Message,CreatedOn")] Feedback feedback)
        {
            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                // Stage the new record.
                _context.Add(feedback);
                // Persist changes to the database.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(feedback);
        }

        // Displays the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            // Return 404 when the record does not exist.
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // Saves the edited record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Email,Message,CreatedOn")] Feedback feedback)
        {
            // Reject a mismatched id.
            if (id != feedback.Id)
            {
                return NotFound();
            }

            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                // Load the existing record to update.
                var existing = await _context.Feedbacks.FindAsync(id);
                // Return 404 when the record does not exist.
                if (existing == null)
                {
                    return NotFound();
                }

                // Copy the edited values onto the tracked entity.
                existing.Name = feedback.Name;
                existing.Email = feedback.Email;
                existing.Message = feedback.Message;
                existing.CreatedOn = feedback.CreatedOn;
                // Persist changes to the database.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(feedback);
        }

        // Shows the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // Deletes the record after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                // Remove the record.
                _context.Feedbacks.Remove(feedback);
            }

            // Persist changes to the database.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper: checks whether the record exists.
        private bool FeedbackExists(int? id)
        {
            return _context.Feedbacks.Any(e => e.Id == id);
        }
    }
}