using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Controllers;

// Customer booking flow: form, live quote, payment, my bookings, reviews and cancel. Login required.
[Authorize]
public class BookingController(Karnel_TravelContext db, BookingService bookings, UserManager<ApplicationUser> users) : Controller
{
    // Shows the booking form, prefilled from the user profile.
    [HttpGet]
    public async Task<IActionResult> Create(BookingType type, int id)
    {
        // Load the bookable item.
        var item = await bookings.GetItemAsync(type, id);
        // Return 404 when the record does not exist.
        if (item == null) return NotFound();

        // Current user, used to prefill contact details.
        var user = await users.GetUserAsync(User);
        // Build the form model.
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

    // Validates and saves the booking, then goes to payment or the thank-you page.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Input")] BookingInput input)
    {
        // Load the bookable item.
        var item = await bookings.GetItemAsync(input.Type, input.ItemId);
        // Return 404 when the record does not exist.
        if (item == null) return NotFound();

        // Build the form model.
        var vm = new BookingFormViewModel { Item = item, Input = input };
        // Redisplay the form on validation errors.
        if (!ModelState.IsValid) return View(vm);

        // Current user id.
        var userId = users.GetUserId(User);
        // Require a signed-in user.
        if (userId == null) return Challenge();

        // Create the booking; the service re-validates price and availability.
        var (booking, error) = await bookings.CreateAsync(input, userId);
        // Show the service error on the form.
        if (booking == null)
        {
            ModelState.AddModelError(string.Empty, error ?? "Could not create your booking. Please try again.");
            return View(vm);
        }

        // Priced bookings go to payment first; others go straight to the thank-you page.
        if (booking.NeedsPayment)
            return RedirectToAction(nameof(Payment), new { id = booking.Id });

        return RedirectToAction(nameof(Success), new { id = booking.Id });
    }

    // Shows the demo payment page.
    [HttpGet]
    public async Task<IActionResult> Payment(int id)
    {
        // Load the booking only if it belongs to the current user.
        var b = await OwnBookingAsync(id);
        // Return 404 when the record does not exist.
        if (b == null) return NotFound();

        // Already paid: go to the thank-you page.
        if (b.PaymentStatus == PaymentStatus.Paid) return RedirectToAction(nameof(Success), new { id });
        // Block payment when it is not allowed for this booking.
        if (!b.CanPayNow)
        {
            TempData["Error"] = "Payment is not available for this booking.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(b);
    }

    // Records the demo payment. The amount always comes from the database.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payment(int id, PaymentInput input)
    {
        // Current user id.
        var userId = users.GetUserId(User);
        // Require a signed-in user.
        if (userId == null) return Challenge();

        // Validate the fields required by the chosen payment method.
        var method = input.Method ?? "";
        if (method == "Card")
        {
            if (string.IsNullOrWhiteSpace(input.CardHolder)) ModelState.AddModelError(nameof(input.CardHolder), "Please enter the name on the card.");
            if (string.IsNullOrWhiteSpace(input.Last4)) ModelState.AddModelError(nameof(input.Last4), "Please enter your card details.");
        }
        else if (method == "JazzCash" || method == "Easypaisa")
        {
            if (string.IsNullOrWhiteSpace(input.WalletNumber)) ModelState.AddModelError(nameof(input.WalletNumber), "Please enter your mobile account number.");
        }

        // Show the first validation error and return to the payment page.
        if (!ModelState.IsValid)
        {
            var first = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
            TempData["Error"] = first ?? "Please check your payment details.";
            return RedirectToAction(nameof(Payment), new { id });
        }

        // Mark the booking as paid (demo only; no real charge).
        var (booking, error) = await bookings.PayAsync(id, userId, method, input.Last4);
        // Return to the details page with the error.
        if (booking == null)
        {
            TempData["Error"] = error ?? "Payment could not be completed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = $"Payment received. Transaction {booking.PaymentReference}.";
        return RedirectToAction(nameof(Success), new { id });
    }

    // Returns the live price and availability as JSON for the booking form.
    [HttpGet]
    public async Task<IActionResult> Quote(BookingType type, int id, DateTime? start, DateTime? end, string? slot, int guests = 1, int units = 1)
    {
        // Load the bookable item.
        var item = await bookings.GetItemAsync(type, id);
        // Return 404 when the record does not exist.
        if (item == null) return NotFound();

        // Calculate the price and check availability on the server.
        var q = await bookings.QuoteAsync(item, start, end, slot, guests, units);
        // Return the fields used by the summary box.
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

    // Thank-you page after a booking or payment.
    public async Task<IActionResult> Success(int id)
    {
        // Load the booking only if it belongs to the current user.
        var b = await OwnBookingAsync(id);
        // Return 404 when the record does not exist.
        if (b == null) return NotFound();
        return View(b);
    }

    // Lists the current user's bookings with status tabs.
    public async Task<IActionResult> My(string? status)
    {
        // Current user id.
        var userId = users.GetUserId(User);
        // All bookings of this user (read-only).
        var all = db.Bookings.AsNoTracking().Where(b => b.UserId == userId);

        // Build the tab counts for each status.
        var vm = new MyBookingsViewModel
        {
            Status = status,
            All = await all.CountAsync(),
            Pending = await all.CountAsync(b => b.Status == BookingStatus.Pending),
            Confirmed = await all.CountAsync(b => b.Status == BookingStatus.Confirmed),
            Done = await all.CountAsync(b => b.Status == BookingStatus.Completed),
            Closed = await all.CountAsync(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Rejected)
        };

        // Apply the selected status tab.
        var filtered = status switch
        {
            "pending" => all.Where(b => b.Status == BookingStatus.Pending),
            "confirmed" => all.Where(b => b.Status == BookingStatus.Confirmed),
            "completed" => all.Where(b => b.Status == BookingStatus.Completed),
            "closed" => all.Where(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Rejected),
            _ => all
        };

        // Load the filtered list, newest first.
        vm.Bookings = await filtered.OrderByDescending(b => b.CreatedOn).ToListAsync();

        // Find which listed bookings already have a review.
        var ids = vm.Bookings.Select(b => b.Id).ToList();
        vm.ReviewedIds = (await db.Feedbacks.AsNoTracking()
            .Where(f => f.BookingId != null && ids.Contains(f.BookingId.Value))
            .Select(f => f.BookingId!.Value).ToListAsync()).ToHashSet();
        return View(vm);
    }

    // Shows one booking in full, with its review if any.
    public async Task<IActionResult> Details(int id)
    {
        // Load the booking only if it belongs to the current user.
        var b = await OwnBookingAsync(id);
        // Return 404 when the record does not exist.
        if (b == null) return NotFound();
        // Attach the review, if one exists.
        ViewBag.Review = await db.Feedbacks.AsNoTracking().FirstOrDefaultAsync(f => f.BookingId == id);
        return View(b);
    }

    // Shows the review form (allowed only after the booking has ended).
    [HttpGet]
    public async Task<IActionResult> Review(int id)
    {
        // Load the booking only if it belongs to the current user.
        var b = await OwnBookingAsync(id);
        // Return 404 when the record does not exist.
        if (b == null) return NotFound();

        // Check whether a review is currently allowed.
        var error = await ReviewBlockReasonAsync(b);
        // Redirect with the reason when a review is not allowed.
        if (error != null)
        {
            TempData[error.Value.IsInfo ? "Success" : "Error"] = error.Value.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(new ReviewFormViewModel { Booking = b });
    }

    // Saves the review. The rules are re-checked on the server.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, [Bind(Prefix = "Input")] ReviewInput input)
    {
        // Load the booking only if it belongs to the current user.
        var b = await OwnBookingAsync(id);
        // Return 404 when the record does not exist.
        if (b == null) return NotFound();

        // Check whether a review is currently allowed.
        var error = await ReviewBlockReasonAsync(b);
        // Redirect with the reason when a review is not allowed.
        if (error != null)
        {
            TempData[error.Value.IsInfo ? "Success" : "Error"] = error.Value.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        // Redisplay the form on validation errors.
        if (!ModelState.IsValid)
            return View(new ReviewFormViewModel { Booking = b, Input = input });

        // Save the review linked to the booking.
        db.Feedbacks.Add(new Feedback
        {
            Name = b.CustomerName,
            Email = b.CustomerEmail,
            Message = input.Message.Trim(),
            Rating = input.Rating,
            UserId = b.UserId,
            BookingId = b.Id,
            ItemType = b.ItemType,
            ItemName = b.ItemName,
            CreatedOn = DateTime.Now
        });

        // Save; the unique index blocks duplicate reviews.
        try
        {
            await db.SaveChangesAsync();
        }
        // Two simultaneous submits hit the unique index; treat as already reviewed.
        catch (DbUpdateException)
        {
            TempData["Success"] = "You have already reviewed this booking.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Thank you! Your review has been published.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Returns why a review is blocked, or null when it is allowed.
    private async Task<(string Message, bool IsInfo)?> ReviewBlockReasonAsync(Booking b)
    {
        // Only one review per booking.
        if (await db.Feedbacks.AnyAsync(f => f.BookingId == b.Id))
            return ("You have already reviewed this booking.", true);

        // Only confirmed bookings can be reviewed.
        if (b.Status is BookingStatus.Cancelled or BookingStatus.Rejected or BookingStatus.Pending)
            return ("Reviews are only available for confirmed bookings.", false);

        // Reviews open after the last day of the booking.
        if (!b.CanReviewNow)
            return ($"You can review this booking after {b.LastDay:dd MMM yyyy}.", false);

        // Review is allowed.
        return null;
    }

    // Cancels the user's own booking before its start date.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        // Current user id.
        var userId = users.GetUserId(User);
        // Require a signed-in user.
        if (userId == null) return Challenge();

        // The service enforces ownership and cancellation rules.
        var (ok, error) = await bookings.CancelAsync(id, userId);
        // Flash a success or error message.
        if (ok) TempData["Success"] = "Your booking has been cancelled.";
        else TempData["Error"] = error ?? "Could not cancel this booking.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // Loads a booking only if it belongs to the signed-in user (guessed ids do not work).
    private async Task<Booking?> OwnBookingAsync(int id)
    {
        // Current user id.
        var userId = users.GetUserId(User);
        return await db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }
}
