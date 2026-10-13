using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin area controller; access limited to the Admin role.
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController(Karnel_TravelContext db, BookingService bookings) : Controller
    {
        // Number of rows shown per page.
        private const int PageSize = 12;

        // Lists bookings with search, filters and pagination.
        public async Task<IActionResult> Index(string? q, string? status, string? type, DateTime? from, DateTime? to, int page = 1)
        {
            // Base query (read-only).
            var query = db.Bookings.AsNoTracking().AsQueryable();

            // Search by code, customer details or item name.
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(b => b.BookingCode.Contains(q) || b.CustomerName.Contains(q)
                                      || b.CustomerEmail.Contains(q) || b.CustomerPhone.Contains(q) || b.ItemName.Contains(q));
            }

            // Filter by status.
            if (Enum.TryParse<BookingStatus>(status, true, out var st))
                query = query.Where(b => b.Status == st);

            // Filter by booking type.
            if (Enum.TryParse<BookingType>(type, true, out var tp))
                query = query.Where(b => b.ItemType == tp);

            // Filter by start date (check-in / travel / visit date).
            if (from.HasValue) query = query.Where(b => b.StartDate >= from.Value.Date);
            if (to.HasValue) query = query.Where(b => b.StartDate <= to.Value.Date);

            // Pagination calculations.
            int total = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            // Fetch the page: pending first, then newest.
            var list = await query
                .OrderByDescending(b => b.Status == BookingStatus.Pending)
                .ThenByDescending(b => b.CreatedOn)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Unfiltered query for summary counts.
            var all = db.Bookings.AsNoTracking();
            // Assemble the view model with filters, paging and summary stats.
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

        // Shows booking details with the customer account.
        public async Task<IActionResult> Details(int id)
        {
            // Load the booking together with its user.
            var b = await db.Bookings.AsNoTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            // Return 404 when the record does not exist.
            if (b == null) return NotFound();
            return View(b);
        }

        // Changes a booking status (confirm, reject, cancel, complete).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, BookingStatus status, string? adminNote, string? returnTo)
        {
            // Apply the change; the service enforces allowed transitions.
            var (ok, error) = await bookings.ChangeStatusAsync(id, status, adminNote);

            // Flash a success or error message.
            if (ok) TempData["Success"] = $"Booking marked as {BookingUi.StatusLabel(status).ToLower()}.";
            else TempData["Error"] = error ?? "Could not update this booking.";

            // Return to the list or the details page, depending on where the request came from.
            if (returnTo == "list") return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
