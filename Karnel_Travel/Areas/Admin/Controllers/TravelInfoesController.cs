
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karnel_Travel.Models;
using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class TravelInfoesController : Controller
    {
        private readonly Karnel_TravelContext _context;

        public TravelInfoesController(Karnel_TravelContext context)
        {
            _context = context;
        }

        // GET: TRAVELINFOS
        public async Task<IActionResult> Index()
        {
            return View(await _context.TravelInfos.ToListAsync());
        }

        // GET: TRAVELINFOS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelinfo = await _context.TravelInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (travelinfo == null)
            {
                return NotFound();
            }

            return View(travelinfo);
        }

        // GET: TRAVELINFOS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TRAVELINFOS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TransportMode,FromCity,ToCity,Fare,Details")] TravelInfo travelinfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(travelinfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(travelinfo);
        }

        // GET: TRAVELINFOS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelinfo = await _context.TravelInfos.FindAsync(id);
            if (travelinfo == null)
            {
                return NotFound();
            }
            return View(travelinfo);
        }

        // POST: TRAVELINFOS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,TransportMode,FromCity,ToCity,Fare,Details")] TravelInfo travelinfo)
        {
            if (id != travelinfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(travelinfo);
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

        // GET: TRAVELINFOS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelinfo = await _context.TravelInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (travelinfo == null)
            {
                return NotFound();
            }

            return View(travelinfo);
        }

        // POST: TRAVELINFOS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var travelinfo = await _context.TravelInfos.FindAsync(id);
            if (travelinfo != null)
            {
                _context.TravelInfos.Remove(travelinfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TravelInfoExists(int? id)
        {
            return _context.TravelInfos.Any(e => e.Id == id);
        }
    }
}