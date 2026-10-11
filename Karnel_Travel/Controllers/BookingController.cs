using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Controllers;

// Booking system (customer side): booking form, live price, my bookings, cancel.
// Login zaroori hai: login na ho to user pehle Login page par jata hai aur wapas isi page par aata hai.
[Authorize]
public class BookingController(Karnel_TravelContext db, BookingService bookings, UserManager<ApplicationUser> users) : Controller
{
    // GET /Booking/Create?type=Hotel&id=5  -> booking form
    [HttpGet]
    public async Task<IActionResult> Create(BookingType type, int id)
    {
        var item = await bookings.GetItemAsync(type, id);
        if (item == null) return NotFound();

        var user = await users.GetUserAsync(User);
        var vm = new BookingFormViewModel
        {
            Item = item,
            Input = new BookingInput
            {
                Type = type,
                ItemId = id,
                CustomerName = user?.FullName ?? "",
                CustomerEmail = user?.Email ?? "",
                CustomerPhone = user?.PhoneNumber ?? "",
                Guests = type == BookingType.Hotel || type == BookingType.Resort ? 2 : 1
            }
        };
        return View(vm);
    }

    // POST /Booking/Create -> booking save karta hai
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Input")] BookingInput input)
    {
        var item = await bookings.GetItemAsync(input.Type, input.ItemId);
        if (item == null) return NotFound();

        var vm = new BookingFormViewModel { Item = item, Input = input };
        if (!ModelState.IsValid) return View(vm);

        var userId = users.GetUserId(User);
        if (userId == null) return Challenge();

        var (booking, error) = await bookings.CreateAsync(input, userId);
        if (booking == null)
        {
            ModelState.AddModelError(string.Empty, error ?? "Could not create your booking. Please try again.");
            return View(vm);
        }

        return RedirectToAction(nameof(Success), new { id = booking.Id });
    }

    // Live price + availability (form ki summary box ke liye JSON)
    [HttpGet]
    public async Task<IActionResult> Quote(BookingType type, int id, DateTime? start, DateTime? end, string? slot, int guests = 1, int units = 1)
    {
        var item = await bookings.GetItemAsync(type, id);
        if (item == null) return NotFound();

        var q = await bookings.QuoteAsync(item, start, end, slot, guests, units);
        return Json(new
        {
            ok = q.Valid,
            error = q.Error,
            nights = q.Nights,
            units = q.Units,
            guests = q.Guests,
            unitPrice = BookingUi.Money(q.UnitPrice),
            discount = q.DiscountPercent,
            subtotal = BookingUi.Money(q.Subtotal),
            total = q.PriceOnRequest ? "Price on confirmation" : BookingUi.Money(q.Total),
            priceOnRequest = q.PriceOnRequest,
            available = q.Available
        });
    }

    // Booking ke baad "Thank you" page
    public async Task<IActionResult> Success(int id)
    {
        var b = await OwnBookingAsync(id);
        if (b == null) return NotFound();
        return View(b);
    }

    // Meri bookings ki list (status tabs ke saath)
    public async Task<IActionResult> My(string? status)
    {
        var userId = users.GetUserId(User);
        var all = db.Bookings.AsNoTracking().Where(b => b.UserId == userId);

        var vm = new MyBookingsViewModel
        {
            Status = status,
            All = await all.CountAsync(),
            Pending = await all.CountAsync(b => b.Status == BookingStatus.Pending),
            Confirmed = await all.CountAsync(b => b.Status == BookingStatus.Confirmed),
            Done = await all.CountAsync(b => b.Status == BookingStatus.Completed),
            Closed = await all.CountAsync(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Rejected)
        };

        var filtered = status switch
        {
            "pending" => all.Where(b => b.Status == BookingStatus.Pending),
            "confirmed" => all.Where(b => b.Status == BookingStatus.Confirmed),
            "completed" => all.Where(b => b.Status == BookingStatus.Completed),
            "closed" => all.Where(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Rejected),
            _ => all
        };

        vm.Bookings = await filtered.OrderByDescending(b => b.CreatedOn).ToListAsync();
        return View(vm);
    }

    // Ek booking ki poori detail
    public async Task<IActionResult> Details(int id)
    {
        var b = await OwnBookingAsync(id);
        if (b == null) return NotFound();
        return View(b);
    }

    // Booking cancel (sirf apni, aur sirf start date se pehle)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = users.GetUserId(User);
        if (userId == null) return Challenge();

        var (ok, error) = await bookings.CancelAsync(id, userId);
        if (ok) TempData["Success"] = "Your booking has been cancelled.";
        else TempData["Error"] = error ?? "Could not cancel this booking.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // Sirf login user ki apni booking (doosre ki booking ID likhne se nahi khulti)
    private async Task<Booking?> OwnBookingAsync(int id)
    {
        var userId = users.GetUserId(User);
        return await db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }
}
