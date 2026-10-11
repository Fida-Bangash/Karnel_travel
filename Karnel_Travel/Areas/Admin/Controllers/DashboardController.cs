using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin Dashboard: sirf "Admin" role wale users hi is page tak pahunch sakte hain
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        // Database ka context (tables tak pahunchne ke liye)
        private readonly Karnel_TravelContext _db;

        // Constructor: ASP.NET khud database context de deta hai (dependency injection)
        public DashboardController(Karnel_TravelContext db)
        {
            _db = db;
        }

        // Dashboard page: har module ki total ginti aur taaza messages/feedback dikhata hai
        public async Task<IActionResult> Index()
        {
            // Har table me kitne records hain (dashboard ke cards ke liye)
            ViewBag.Spots = await _db.TouristSpots.CountAsync();
            ViewBag.TravelInfos = await _db.TravelInfos.CountAsync();
            ViewBag.Hotels = await _db.Hotels.CountAsync();
            ViewBag.Restaurants = await _db.Restaurants.CountAsync();
            ViewBag.Resorts = await _db.Resorts.CountAsync();
            ViewBag.Packages = await _db.TourPackages.CountAsync();
            ViewBag.Messages = await _db.ContactMessages.CountAsync();
            ViewBag.Feedback = await _db.Feedbacks.CountAsync();

            // Bookings: total, pending (admin ke action ka intezar), aur aaj/aage ki revenue
            ViewBag.Bookings = await _db.Bookings.CountAsync();
            ViewBag.PendingBookings = await _db.Bookings.CountAsync(b => b.Status == Karnel_Travel.Models.BookingStatus.Pending);
            ViewBag.BookingRevenue = await _db.Bookings
                .Where(b => b.Status == Karnel_Travel.Models.BookingStatus.Confirmed || b.Status == Karnel_Travel.Models.BookingStatus.Completed)
                .SumAsync(b => b.TotalPrice);

            // Sab se nayi 5 bookings
            ViewBag.RecentBookings = await _db.Bookings
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedOn)
                .Take(5)
                .ToListAsync();

            // Sab se naye 5 contact messages
            ViewBag.RecentMessages = await _db.ContactMessages
                .OrderByDescending(m => m.CreatedOn)
                .Take(5)
                .ToListAsync();

            // Sab se naye 5 feedbacks
            ViewBag.RecentFeedback = await _db.Feedbacks
                .OrderByDescending(f => f.CreatedOn)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
