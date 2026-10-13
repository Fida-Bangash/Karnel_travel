using Karnel_Travel.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Karnel_Travel.Data;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Controllers
{
    // Public website pages: home, about, services, destinations, contact and reviews.
    public class HomeController : Controller
    {
        // Database context.
        private readonly Karnel_TravelContext _db;

        // Constructor: dependencies are injected by ASP.NET.
        public HomeController(Karnel_TravelContext db)
        {
            _db = db;
        }

        // Destination page: lists all tourist spots.
        public async Task<IActionResult> Destination()
        {
            var spots = await _db.TouristSpots.ToListAsync();

            return View(spots);
        }

        // Home page: newest spots and recent customer reviews.
        public async Task<IActionResult> Index()
        {
            // Four newest spots for the Popular Destinations section.
            ViewBag.PopularSpots = await _db.TouristSpots
                .OrderByDescending(s => s.Id)
                .Take(4)
                .ToListAsync();

            // Reviews come only from completed bookings (rated feedback).
            var reviews = _db.Feedbacks.AsNoTracking().Where(f => f.BookingId != null && f.Rating > 0);
            // Latest 6 reviews.
            ViewBag.Reviews = await reviews.OrderByDescending(f => f.CreatedOn).Take(6).ToListAsync();
            // Review count and average rating.
            int reviewCount = await reviews.CountAsync();
            ViewBag.ReviewCount = reviewCount;
            ViewBag.ReviewAverage = reviewCount > 0 ? await reviews.AverageAsync(f => (double)f.Rating) : 0.0;

            return View();
        }

        // About Us page.
        public IActionResult About()
        {
            return View();
        }

        // Services page.
        public IActionResult Service()
        {
            return View();
        }

        // True for fetch/AJAX requests, which get JSON instead of a page reload.
        private bool IsAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        // Converts validation errors into JSON for AJAX forms.
        private JsonResult ValidationErrorsJson()
        {
            // Map each field to its error messages.
            var errors = ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

            return Json(new { success = false, message = "Please fix the errors below.", errors });
        }

        // Request types accepted from the contact form.
        private static readonly string[] AllowedRequestTypes =
        {
            "General Inquiry", "Custom Tour Request"
        };

        // Shows the contact form, prefilled when coming from an order or booking button.
        [HttpGet]
        public IActionResult Contact(string? type, string? item)
        {
            // Start with an empty message.
            var model = new ContactMessage();

            // Prefill the subject with the item name (max 150 characters).
            if (!string.IsNullOrWhiteSpace(item))
            {
                model.Subject = item.Length > 150 ? item[..150] : item;
            }

            // Preselect the request type only if it is allowed.
            ViewBag.RequestType = AllowedRequestTypes.Contains(type) ? type : "General Inquiry";

            return View(model);
        }

        // Validates and saves the contact message. [Bind] limits the accepted fields.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact([Bind("Name,Email,Phone,Subject,Message")] ContactMessage model, string? requestType)
        {
            // Return validation errors (JSON for AJAX, otherwise redisplay the form).
            if (!ModelState.IsValid)
            {
                // AJAX: return errors as JSON without a page reload.
                if (IsAjax) return ValidationErrorsJson();

                // Keep the selected request type when redisplaying.
                ViewBag.RequestType = requestType;
                return View(model);
            }

            // Tag the subject so admins can tell orders from inquiries.
            if (AllowedRequestTypes.Contains(requestType) && requestType != "General Inquiry")
            {
                var tagged = $"[{requestType}] {model.Subject}";
                model.Subject = tagged.Length > 150 ? tagged[..150] : tagged;
            }

            // Save the message.
            _db.ContactMessages.Add(model);
            // Persist changes to the database.
            await _db.SaveChangesAsync();

            // Confirmation text shown to the user.
            const string successMessage = "Thank you! Your request has been sent. We will contact you soon.";

            // AJAX: return success JSON; the form stays on the page.
            if (IsAjax) return Json(new { success = true, message = successMessage });

            // Show the message on the next page load.
            TempData["Success"] = successMessage;

            // Post/Redirect/Get to prevent duplicate submissions.
            return RedirectToAction(nameof(Contact));
        }

        // Lists customer reviews (latest 100, from completed bookings only).
        public async Task<IActionResult> Reviews()
        {
            var list = await _db.Feedbacks.AsNoTracking()
                .Where(f => f.BookingId != null && f.Rating > 0)
                .OrderByDescending(f => f.CreatedOn)
                .Take(100)
                .ToListAsync();

            return View(list);
        }

        // Privacy Policy page.
        public IActionResult Privacy()
        {
            return View();
        }

        // Error page that shows the request id for troubleshooting.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}