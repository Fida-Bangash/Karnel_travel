
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karnel_Travel.Models;
using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactMessagesController : Controller
    {
        private readonly Karnel_TravelContext _context;

        public ContactMessagesController(Karnel_TravelContext context)
        {
            _context = context;
        }

        // GET: CONTACTMESSAGES
        public async Task<IActionResult> Index()
        {
            return View(await _context.ContactMessages.ToListAsync());
        }

        // GET: CONTACTMESSAGES/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactmessage = await _context.ContactMessages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contactmessage == null)
            {
                return NotFound();
            }

            return View(contactmessage);
        }

        // GET: CONTACTMESSAGES/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CONTACTMESSAGES/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Phone,Subject,Message,CreatedOn,IsRead")] ContactMessage contactmessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contactmessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contactmessage);
        }

        // GET: CONTACTMESSAGES/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactmessage = await _context.ContactMessages.FindAsync(id);
            if (contactmessage == null)
            {
                return NotFound();
            }
            return View(contactmessage);
        }

        // POST: CONTACTMESSAGES/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Email,Phone,Subject,Message,CreatedOn,IsRead")] ContactMessage contactmessage)
        {
            if (id != contactmessage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contactmessage);
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

        // GET: CONTACTMESSAGES/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactmessage = await _context.ContactMessages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contactmessage == null)
            {
                return NotFound();
            }

            return View(contactmessage);
        }

        // POST: CONTACTMESSAGES/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var contactmessage = await _context.ContactMessages.FindAsync(id);
            if (contactmessage != null)
            {
                _context.ContactMessages.Remove(contactmessage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactMessageExists(int? id)
        {
            return _context.ContactMessages.Any(e => e.Id == id);
        }
    }
}