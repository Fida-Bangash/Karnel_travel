using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin ke liye Bookings page: list + filters, detail, aur Confirm / Reject / Cancel / Complete
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController(Karnel_TravelContext db, BookingService bookings) : Controller
    {
        private const int PageSize = 12;

        // GET: Admin/Bookings?status=pending&type=Hotel&q=ali&from=2026-10-01&to=2026-10-31&page=2
        public async Task<IActionResult> Index(string? q, string? status, string? type, DateTime? from, DateTime? to, int page = 1)
        {
            var query = db.Bookings.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(b => b.BookingCode.Contains(q) || b.CustomerName.Contains(q)
                                      || b.CustomerEmail.Contains(q) || b.CustomerPhone.Contains(q) || b.ItemName.Contains(q));
            }

            if (Enum.TryParse<BookingStatus>(status, true, out var st))
                query = query.Where(b => b.Status == st);

            if (Enum.TryParse<BookingType>(type, true, out var tp))
                query = query.Where(b => b.ItemType == tp);

            // Date filter booking ki start date par (check-in / travel / visit date)
            if (from.HasValue) query = query.Where(b => b.StartDate >= from.Value.Date);
            if (to.HasValue) query = query.Where(b => b.StartDate <= to.Value.Date);

            int total = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var list = await query
                .OrderByDescending(b => b.Status == BookingStatus.Pending)   // pending sab se upar
                .ThenByDescending(b => b.CreatedOn)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var all = db.Bookings.AsNoTracking();
            var vm = new AdminBookingsViewModel
            {
                Bookings = list,
                Q = q,
                Status = status,
                Type = type,
                From = from,
                To = to,
                Page = page,
                PageSize = PageSize,
                TotalPages = totalPages,
                TotalItems = total,
                AllCount = await all.CountAsync(),
                PendingCount = await all.CountAsync(b => b.Status == BookingStatus.Pending),
                ConfirmedCount = await all.CountAsync(b => b.Status == BookingStatus.Confirmed),
                Revenue = await all.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                                   .SumAsync(b => b.TotalPrice)
            };
            return View(vm);
        }

        // GET: Admin/Bookings/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var b = await db.Bookings.AsNoTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return NotFound();
            return View(b);
        }

        // POST: Admin/Bookings/SetStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, BookingStatus status, string? adminNote, string? returnTo)
        {
            var (ok, error) = await bookings.ChangeStatusAsync(id, status, adminNote);

            if (ok) TempData["Success"] = $"Booking marked as {BookingUi.StatusLabel(status).ToLower()}.";
            else TempData["Error"] = error ?? "Could not update this booking.";

            // List se aaye to list par, detail se aaye to detail par wapas
            if (returnTo == "list") return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
