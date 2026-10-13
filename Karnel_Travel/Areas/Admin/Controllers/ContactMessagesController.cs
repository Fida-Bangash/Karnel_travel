
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
    public class ContactMessagesController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _context;

        // Constructor: dependencies are injected by ASP.NET.
        public ContactMessagesController(Karnel_TravelContext context)
        {
            _context = context;
        }

        // Lists records.
        public async Task<IActionResult> Index()
        {
            return View(await _context.ContactMessages.ToListAsync());
        }

        // Shows a single record.
        public async Task<IActionResult> Details(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var contactmessage = await _context.ContactMessages
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (contactmessage == null)
            {
                return NotFound();
            }

            return View(contactmessage);
        }

        // Displays the create form.
        public IActionResult Create()
        {
            return View();
        }

        // Validates input and saves the new record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Phone,Subject,Message,CreatedOn,IsRead")] ContactMessage contactmessage)
        {
            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                // Stage the new record.
                _context.Add(contactmessage);
                // Persist changes to the database.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contactmessage);
        }

        // Displays the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var contactmessage = await _context.ContactMessages.FindAsync(id);
            // Return 404 when the record does not exist.
            if (contactmessage == null)
            {
                return NotFound();
            }
            return View(contactmessage);
        }

        // Saves the edited record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Email,Phone,Subject,Message,CreatedOn,IsRead")] ContactMessage contactmessage)
        {
            // Reject a mismatched id.
            if (id != contactmessage.Id)
            {
                return NotFound();
            }

            // Proceed only when validation passes.
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contactmessage);
                    // Persist changes to the database.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactMessageExists(contactmessage.Id))
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
            return View(contactmessage);
        }

        // Shows the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            // Reject requests without an id.
            if (id == null)
            {
                return NotFound();
            }

            var contactmessage = await _context.ContactMessages
                .FirstOrDefaultAsync(m => m.Id == id);
            // Return 404 when the record does not exist.
            if (contactmessage == null)
            {
                return NotFound();
            }

            return View(contactmessage);
        }

        // Deletes the record after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var contactmessage = await _context.ContactMessages.FindAsync(id);
            if (contactmessage != null)
            {
                // Remove the record.
                _context.ContactMessages.Remove(contactmessage);
            }

            // Persist changes to the database.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper: checks whether the record exists.
        private bool ContactMessageExists(int? id)
        {
            return _context.ContactMessages.Any(e => e.Id == id);
        }
    }
}