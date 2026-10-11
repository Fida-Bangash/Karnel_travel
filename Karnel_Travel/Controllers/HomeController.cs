using Karnel_Travel.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Karnel_Travel.Data;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Controllers
{
    // Public website ke main pages: Home, About, Services, Destination, Contact, Privacy
    public class HomeController : Controller
    {
        // Database ka context (tables tak pahunchne ke liye)
        private readonly Karnel_TravelContext _db;

        // Constructor: ASP.NET khud database context de deta hai (dependency injection)
        public HomeController(Karnel_TravelContext db)
        {
            _db = db;
        }

        // Destination page: saare tourist spots database se dikhata hai
        public async Task<IActionResult> Destination()
        {
            var spots = await _db.TouristSpots.ToListAsync();

            return View(spots);
        }

        // Home page: "Popular Destination" section ke liye sab se naye 4 tourist spots
        public async Task<IActionResult> Index()
        {
            ViewBag.PopularSpots = await _db.TouristSpots
                .OrderByDescending(s => s.Id)
                .Take(4)
                .ToListAsync();

            return View();
        }

        // About Us page (company ki jankari)
        public IActionResult About()
        {
            return View();
        }

        // Services page
        public IActionResult Service()
        {
            return View();
        }

        // True agar request JavaScript (fetch/AJAX) se aayi hai, tab page reload nahi hota, JSON wapas jata hai
        private bool IsAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        // Validation ke errors ko JSON me badalta hai: { success:false, errors:{ Name:["..."], Email:["..."] } }
        private JsonResult ValidationErrorsJson()
        {
            var errors = ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

            return Json(new { success = false, message = "Please fix the errors below.", errors });
        }

        // Contact form me jo request types allowed hain (order, booking ya general inquiry)
        private static readonly string[] AllowedRequestTypes =
        {
            "General Inquiry", "Custom Tour Request"
        };

        // Contact page (GET): form dikhata hai
        // "Order Now" / "Book Now" button se aaye to type aur item (package/hotel ka naam) pehle se bhar deta hai
        [HttpGet]
        public IActionResult Contact(string? type, string? item)
        {
            var model = new ContactMessage();

            // Item ka naam subject me daal do (Subject ki limit 150 characters hai)
            if (!string.IsNullOrWhiteSpace(item))
            {
                model.Subject = item.Length > 150 ? item[..150] : item;
            }

            // Sirf allowed type hi dropdown me select hoga
            ViewBag.RequestType = AllowedRequestTypes.Contains(type) ? type : "General Inquiry";

            return View(model);
        }

        // Contact form submit (POST): message database me save karta hai
        // [Bind] sirf wahi fields leta hai jo form me hain (extra fields ignore hote hain)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact([Bind("Name,Email,Phone,Subject,Message")] ContactMessage model, string? requestType)
        {
            // Agar data galat ho (khali ya galat email) to form wapas dikhao errors ke saath
            if (!ModelState.IsValid)
            {
                // AJAX request ho to sirf errors JSON me bhejo (page reload nahi hoga)
                if (IsAjax) return ValidationErrorsJson();

                ViewBag.RequestType = requestType;
                return View(model);
            }

            // Agar customer ne order/booking chuna hai to subject ke shuru me tag laga do,
            // taake admin ko messages list me hi pata chal jaye ke ye order hai ya inquiry
            if (AllowedRequestTypes.Contains(requestType) && requestType != "General Inquiry")
            {
                var tagged = $"[{requestType}] {model.Subject}";
                model.Subject = tagged.Length > 150 ? tagged[..150] : tagged;
            }

            _db.ContactMessages.Add(model);
            await _db.SaveChangesAsync();

            const string successMessage = "Thank you! Your request has been sent. We will contact you soon.";

            // AJAX request ho to success JSON bhejo, form wahi rehta hai
            if (IsAjax) return Json(new { success = true, message = successMessage });

            // Success message agle page par dikhane ke liye
            TempData["Success"] = successMessage;

            return RedirectToAction(nameof(Contact));
        }

        // Feedback form submit (POST): feedback database me save karta hai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback([Bind("Name,Email,Message")] Feedback model)
        {
            if (!ModelState.IsValid)
            {
                if (IsAjax) return ValidationErrorsJson();

                TempData["Error"] = "Please enter valid feedback details.";
                return RedirectToAction(nameof(Contact));
            }

            _db.Feedbacks.Add(model);
            await _db.SaveChangesAsync();

            if (IsAjax) return Json(new { success = true, message = "Thanks for your feedback!" });

            TempData["Success"] = "Thanks for your feedback!";

            return RedirectToAction(nameof(Contact));
        }

        // Privacy Policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Error page: koi masla aaye to ye dikhta hai
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}