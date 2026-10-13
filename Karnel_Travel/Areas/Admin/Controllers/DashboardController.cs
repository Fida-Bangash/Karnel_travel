using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin area controller; access limited to the Admin role.
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _db;

        // Constructor: dependencies are injected by ASP.NET.
        public DashboardController(Karnel_TravelContext db)
        {
            _db = db;
        }

        // Loads dashboard statistics and recent activity.
        public async Task<IActionResult> Index()
        {
            // Record counts for the summary cards.
            ViewBag.Spots = await _db.TouristSpots.CountAsync();
            ViewBag.TravelInfos = await _db.TravelInfos.CountAsync();
            ViewBag.Hotels = await _db.Hotels.CountAsync();
            ViewBag.Restaurants = await _db.Restaurants.CountAsync();
            ViewBag.Resorts = await _db.Resorts.CountAsync();
            ViewBag.Packages = await _db.TourPackages.CountAsync();
            ViewBag.Messages = await _db.ContactMessages.CountAsync();
            ViewBag.Feedback = await _db.Feedbacks.CountAsync();

            // Booking totals: all, pending, and revenue from confirmed/completed bookings.
            ViewBag.Bookings = await _db.Bookings.CountAsync();
            ViewBag.PendingBookings = await _db.Bookings.CountAsync(b => b.Status == Karnel_Travel.Models.BookingStatus.Pending);
            ViewBag.BookingRevenue = await _db.Bookings
                .Where(b => b.Status == Karnel_Travel.Models.BookingStatus.Confirmed || b.Status == Karnel_Travel.Models.BookingStatus.Completed)
                .SumAsync(b => b.TotalPrice);

            // Latest 5 bookings.
            ViewBag.RecentBookings = await _db.Bookings
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedOn)
                .Take(5)
                .ToListAsync();

            // Latest 5 contact messages.
            ViewBag.RecentMessages = await _db.ContactMessages
                .OrderByDescending(m => m.CreatedOn)
                .Take(5)
                .ToListAsync();

            // Latest 5 feedback entries.
            ViewBag.RecentFeedback = await _db.Feedbacks
                .OrderByDescending(f => f.CreatedOn)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
