using System.Security.Cryptography;
using Karnel_Travel.Models;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Data;

// Core booking logic: lookup, pricing, availability, create, cancel, pay and status changes. Price and availability are always computed on the server.
public class BookingService(Karnel_TravelContext db)
{
    // Maximum guests per room or table.
    public const int GuestsPerUnit = 4;

    // Restaurant time slots (one hour each).
    public static readonly string[] RestaurantSlots =
    {
        "12:00", "13:00", "14:00", "15:00", "18:00", "19:00", "20:00", "21:00", "22:00"
    };

    // Serialises booking creation so the last room or table cannot be double-booked (single server only).
    private static readonly SemaphoreSlim CreateGate = new(1, 1);

    // Finds a bookable item by type and id and maps it to a common model.
    public async Task<BookableItem?> GetItemAsync(BookingType type, int id)
    {
        // Map each booking type to its source table.
        switch (type)
        {
            // Hotel: nightly price; rooms act as capacity.
            case BookingType.Hotel:
                return await db.Hotels.AsNoTracking().Where(h => h.Id == id).Select(h => new BookableItem
                {
                    Type = type, Id = h.Id, Name = h.Name, Location = h.Location, UnitPrice = h.Price,
                    Image = h.ImagePath, DefaultImage = "/img/destination-1.jpg", Description = h.Description,
                    Rating = h.Rating, Capacity = h.Quantity, IsAvailable = h.IsAvailable && h.Quantity > 0
                }).FirstOrDefaultAsync();

            // Resort: same model as a hotel.
            case BookingType.Resort:
                return await db.Resorts.AsNoTracking().Where(r => r.Id == id).Select(r => new BookableItem
                {
                    Type = type, Id = r.Id, Name = r.Name, Location = r.Location, UnitPrice = r.Price,
                    Image = r.ImagePath, DefaultImage = "/img/destination-2.jpg", Description = r.Description,
                    Rating = r.Rating, Capacity = r.Quantity, IsAvailable = r.IsAvailable && r.Quantity > 0
                }).FirstOrDefaultAsync();

            // Restaurant: price is the average meal per guest; tables act as capacity.
            case BookingType.Restaurant:
                return await db.Restaurants.AsNoTracking().Where(r => r.Id == id).Select(r => new BookableItem
                {
                    Type = type, Id = r.Id, Name = r.Name, Location = r.Location, UnitPrice = r.Price,
                    Image = r.ImagePath, DefaultImage = "/img/destination-3.jpg", Description = r.Description,
                    Rating = r.Rating, Capacity = r.Quantity, IsAvailable = r.IsAvailable && r.Quantity > 0,
                    Extra = r.Cuisine
                }).FirstOrDefaultAsync();

            // Transport: fare per passenger.
            case BookingType.Transport:
                var t = await db.TravelInfos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (t == null) return null;
                return new BookableItem
                {
                    Type = type, Id = t.Id, Name = $"{t.TransportMode}: {t.FromCity} → {t.ToCity}",
                    Location = $"{t.FromCity} → {t.ToCity}", UnitPrice = t.Fare, DefaultImage = "/img/destination-4.jpg",
                    Description = t.Details, IsAvailable = true, Extra = t.TransportMode
                };

            // Package: price per person with optional discount and expiry date.
            case BookingType.Package:
                var p = await db.TourPackages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (p == null) return null;
                return new BookableItem
                {
                    Type = type, Id = p.Id, Name = p.Title, Location = "Tour Package", UnitPrice = p.Price,
                    Image = p.ImagePath, DefaultImage = "/img/destination-2.jpg", Description = p.Details,
                    DiscountPercent = p.DiscountPercent, ValidTill = p.ValidTill,
                    IsAvailable = p.ValidTill == null || p.ValidTill.Value.Date >= DateTime.Today
                };

            // Tourist spot: free entry, so the price is on request.
            case BookingType.Spot:
                var s = await db.TouristSpots.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (s == null) return null;
                return new BookableItem
                {
                    Type = type, Id = s.Id, Name = s.Name, Location = s.Location, UnitPrice = 0,
                    Image = s.ImagePath, DefaultImage = "/img/destination-1.jpg", Description = s.Description,
                    IsAvailable = true, Extra = s.Category
                };
        }
        // Unknown booking type.
        return null;
    }

    // Counts the rooms or tables still free: stays are checked night by night, restaurants by date and time slot.
    public async Task<int> GetAvailableUnitsAsync(BookableItem item, DateTime start, DateTime? end, string? slot)
    {
        // Items without inventory are always available.
        if (!item.TracksInventory) return int.MaxValue;

        // Use the date part only.
        var s = start.Date;

        // Restaurant: sum the tables booked for the same date and slot.
        if (item.Type == BookingType.Restaurant)
        {
            var used = await db.Bookings.AsNoTracking()
                .Where(b => b.ItemType == item.Type && b.ItemId == item.Id
                            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                            && b.StartDate == s && b.TimeSlot == slot)
                .SumAsync(b => b.Units);
            return Math.Max(0, item.Capacity - used);
        }

        // Stays: load active (pending or confirmed) bookings that overlap the range.
        var e = (end ?? s.AddDays(1)).Date;
        var rows = await db.Bookings.AsNoTracking()
            .Where(b => b.ItemType == item.Type && b.ItemId == item.Id
                        && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                        && b.StartDate < e && b.EndDate > s)
            .Select(b => new { b.StartDate, b.EndDate, b.Units })
            .ToListAsync();

        // Find the busiest night; it limits what can be booked.
        int maxUsed = 0;
        for (var d = s; d < e; d = d.AddDays(1))
        {
            int used = rows.Where(r => r.StartDate <= d && r.EndDate > d).Sum(r => r.Units);
            if (used > maxUsed) maxUsed = used;
        }
        return Math.Max(0, item.Capacity - maxUsed);
    }

    // Validates the input and returns the price and availability.
    public async Task<BookingQuote> QuoteAsync(BookableItem item, DateTime? start, DateTime? end, string? slot, int guests, int units)
    {
        // Start the quote with the item rate.
        var q = new BookingQuote { Guests = guests, UnitPrice = item.UnitPrice, DiscountPercent = item.DiscountPercent };

        // Reject unavailable or expired items.
        if (!item.IsAvailable)
            return Fail(q, item.Type == BookingType.Package ? "This package has expired." : "This item is not available for booking right now.");

        // Date rules: required, not in the past, at most one year ahead.
        var today = DateTime.Today;
        if (start == null) return Fail(q, "Please choose a date.");
        var s = start.Value.Date;
        if (s < today) return Fail(q, "The date cannot be in the past.");
        if (s > today.AddYears(1)) return Fail(q, "Bookings can be made up to one year in advance.");

        // Apply the rules for each booking type.
        switch (item.Type)
        {
            // Hotels and resorts: check-out after check-in, up to 30 nights, 1-10 rooms, 4 guests per room.
            case BookingType.Hotel:
            case BookingType.Resort:
                if (end == null || end.Value.Date <= s) return Fail(q, "Check-out must be after check-in.");
                q.Nights = (end.Value.Date - s).Days;
                if (q.Nights > 30) return Fail(q, "You can book up to 30 nights at a time.");
                if (units < 1 || units > 10) return Fail(q, "You can book 1 to 10 rooms at a time.");
                if (guests < units) return Fail(q, "Each room needs at least 1 guest.");
                if (guests > units * GuestsPerUnit) return Fail(q, $"A room fits up to {GuestsPerUnit} guests. Add more rooms for {guests} guests.");
                q.Units = units;
                q.Subtotal = item.UnitPrice * units * q.Nights;
                break;

            // Restaurants: valid time slot, not already passed, 1-40 guests; one table per 4 guests.
            case BookingType.Restaurant:
                if (string.IsNullOrWhiteSpace(slot) || !RestaurantSlots.Contains(slot)) return Fail(q, "Please choose a time slot.");
                if (s == today && TimeSpan.TryParse(slot, out var slotTime) && slotTime <= DateTime.Now.TimeOfDay)
                    return Fail(q, "That time slot has already passed today.");
                if (guests < 1 || guests > 40) return Fail(q, "A table reservation can have 1 to 40 guests.");
                q.Units = (int)Math.Ceiling(guests / (double)GuestsPerUnit);
                q.Subtotal = item.UnitPrice * guests;
                break;

            // Transport: 1-20 passengers.
            case BookingType.Transport:
                if (guests < 1 || guests > 20) return Fail(q, "You can book 1 to 20 passengers at a time.");
                q.Units = 1;
                q.Subtotal = item.UnitPrice * guests;
                break;

            // Packages: 1-50 persons.
            case BookingType.Package:
                if (guests < 1 || guests > 50) return Fail(q, "A package can be booked for 1 to 50 persons.");
                q.Units = 1;
                q.Subtotal = item.UnitPrice * guests;
                break;

            // Spots: 1-50 persons, no charge.
            default:
                if (guests < 1 || guests > 50) return Fail(q, "You can book for 1 to 50 persons.");
                q.Units = 1;
                q.Subtotal = 0;
                break;
        }

        // Spots and unpriced items show "price on request".
        q.PriceOnRequest = item.Type == BookingType.Spot || item.UnitPrice <= 0;
        // Packages apply their discount percentage.
        q.Total = item.Type == BookingType.Package
            ? Math.Round(q.Subtotal * (100 - item.DiscountPercent) / 100m, 2)
            : q.Subtotal;

        // Check that enough rooms or tables are free.
        if (item.TracksInventory)
        {
            q.Available = await GetAvailableUnitsAsync(item, s, end, slot);
            if (q.Available < q.Units)
            {
                var what = item.Type == BookingType.Restaurant ? "table" : "room";
                return Fail(q, q.Available == 0
                    ? (item.Type == BookingType.Restaurant ? "This time slot is fully booked. Please try another slot." : "Sorry, no rooms are available for these dates.")
                    : $"Only {q.Available} {what}{(q.Available == 1 ? "" : "s")} left for this {(item.Type == BookingType.Restaurant ? "slot" : "date range")}.");
            }
        }

        // All checks passed.
        q.Valid = true;
        return q;
    }

    // Helper: marks the quote invalid with an error message.
    private static BookingQuote Fail(BookingQuote q, string error)
    {
        q.Valid = false;
        q.Error = error;
        return q;
    }

    // Creates a pending booking after re-validating everything on the server.
    public async Task<(Booking? Booking, string? Error)> CreateAsync(BookingInput input, string userId)
    {
        // Take the lock so only one booking is created at a time.
        await CreateGate.WaitAsync();
        try
        {
            // Reload the item and recompute the quote; client values are not trusted.
            var item = await GetItemAsync(input.Type, input.ItemId);
            if (item == null) return (null, "This item no longer exists.");

            var quote = await QuoteAsync(item, input.StartDate, input.EndDate, input.TimeSlot, input.Guests, input.Units);
            // Stop on any validation or availability error.
            if (!quote.Valid) return (null, quote.Error);

            // Normalise the date and detect stay-type bookings.
            var start = input.StartDate!.Value.Date;
            var isStay = item.Type is BookingType.Hotel or BookingType.Resort;
            var slot = item.Type == BookingType.Restaurant ? input.TimeSlot : null;

            // Block duplicate active bookings for the same item and date.
            var duplicate = await db.Bookings.AnyAsync(b => b.UserId == userId && b.ItemType == item.Type && b.ItemId == item.Id
                && b.StartDate == start && b.TimeSlot == slot
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
            if (duplicate) return (null, "You already have an active booking for this item on this date. Check My Bookings.");

            // Limit each user to 5 pending bookings (spam guard).
            var pending = await db.Bookings.CountAsync(b => b.UserId == userId && b.Status == BookingStatus.Pending);
            if (pending >= 5) return (null, "You already have 5 pending bookings. Please wait until they are confirmed.");

            // Build the booking; prices come from the server quote.
            var booking = new Booking
            {
                BookingCode = await NewCodeAsync(),
                UserId = userId,
                ItemType = item.Type,
                ItemId = item.Id,
                ItemName = Trim(item.Name, 150),
                ItemLocation = Trim(item.Location, 200),
                CustomerName = Trim(input.CustomerName.Trim(), 100),
                CustomerEmail = input.CustomerEmail.Trim(),
                CustomerPhone = input.CustomerPhone.Trim(),
                StartDate = start,
                EndDate = isStay ? input.EndDate!.Value.Date : null,
                TimeSlot = slot,
                Guests = quote.Guests,
                Units = quote.Units,
                Nights = quote.Nights,
                UnitPrice = item.UnitPrice,
                DiscountPercent = item.Type == BookingType.Package ? item.DiscountPercent : 0,
                TotalPrice = quote.Total,
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                Status = BookingStatus.Pending,
                CreatedOn = DateTime.Now
            };

            // Save the booking.
            db.Bookings.Add(booking);
            // Persist changes to the database.
            await db.SaveChangesAsync();
            return (booking, null);
        }
        // Always release the lock.
        finally
        {
            CreateGate.Release();
        }
    }

    // Lets a user cancel their own pending booking before its start date.
    public async Task<(bool Ok, string? Error)> CancelAsync(int id, string userId)
    {
        // Load the booking only if it belongs to the user.
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (b == null) return (false, "Booking not found.");
        // Confirmed bookings must be cancelled through support.
        if (b.Status == BookingStatus.Confirmed)
            return (false, "This booking is confirmed and cannot be cancelled online. Please contact us.");
        // Only pending bookings can be cancelled online.
        if (b.Status != BookingStatus.Pending) return (false, "This booking can no longer be cancelled.");
        // Cancellation must happen before the start date.
        if (b.StartDate.Date <= DateTime.Today)
            return (false, "Bookings can only be cancelled before the start date. Please contact us.");

        // Cancel the booking and mark any payment as refunded (demo).
        b.Status = BookingStatus.Cancelled;
        if (b.PaymentStatus == PaymentStatus.Paid) b.PaymentStatus = PaymentStatus.Refunded;
        b.UpdatedOn = DateTime.Now;
        // Persist changes to the database.
        await db.SaveChangesAsync();
        return (true, null);
    }

    // Supported demo payment methods.
    public static readonly string[] PaymentMethods = { "Card", "JazzCash", "Easypaisa" };

    // Records a demo payment (no real money is charged).
    public async Task<(Booking? Booking, string? Error)> PayAsync(int id, string userId, string method, string? last4)
    {
        // Load the booking only if it belongs to the user.
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (b == null) return (null, "Booking not found.");
        // Prevent double payment.
        if (b.PaymentStatus == PaymentStatus.Paid) return (null, "This booking is already paid.");
        // Only payable bookings can be paid.
        if (!b.CanPayNow) return (null, "Payment is not available for this booking.");
        // Accept only supported methods.
        if (!PaymentMethods.Contains(method)) return (null, "Please choose a valid payment method.");

        // Store the payment (card: last 4 digits only) with a unique transaction id.
        b.PaymentStatus = PaymentStatus.Paid;
        b.PaymentMethod = method;
        b.PaymentLast4 = method == "Card" ? last4 : null;
        b.PaymentReference = await NewTxnAsync();
        b.PaidOn = DateTime.Now;
        b.UpdatedOn = DateTime.Now;
        // Persist changes to the database.
        await db.SaveChangesAsync();
        return (b, null);
    }

    // Allowed transitions: pending to confirmed/rejected/cancelled; confirmed to completed/cancelled.
    public static bool CanMove(BookingStatus from, BookingStatus to) => from switch
    {
        BookingStatus.Pending => to is BookingStatus.Confirmed or BookingStatus.Rejected or BookingStatus.Cancelled,
        BookingStatus.Confirmed => to is BookingStatus.Completed or BookingStatus.Cancelled,
        _ => false
    };

    // Admin status change with transition rules and an optional note.
    public async Task<(bool Ok, string? Error)> ChangeStatusAsync(int id, BookingStatus target, string? adminNote)
    {
        // Load the booking.
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return (false, "Booking not found.");
        // Reject invalid transitions.
        if (!CanMove(b.Status, target))
            return (false, $"A {BookingUi.StatusLabel(b.Status).ToLower()} booking cannot be changed to {BookingUi.StatusLabel(target).ToLower()}.");
        // Future bookings cannot be marked as completed.
        if (target == BookingStatus.Completed && b.StartDate.Date > DateTime.Today)
            return (false, "A future booking cannot be marked as completed.");

        // Apply the new status.
        b.Status = target;
        // Refund a paid booking when it is cancelled or rejected (demo).
        if ((target == BookingStatus.Cancelled || target == BookingStatus.Rejected) && b.PaymentStatus == PaymentStatus.Paid)
            b.PaymentStatus = PaymentStatus.Refunded;
        // Store the admin note shown to the customer (max 500 characters).
        if (!string.IsNullOrWhiteSpace(adminNote)) b.AdminNote = Trim(adminNote.Trim(), 500);
        b.UpdatedOn = DateTime.Now;
        // Persist changes to the database.
        await db.SaveChangesAsync();
        return (true, null);
    }

    // Generates a unique booking code such as KT-yyMMdd-XXXX (no confusing characters).
    private async Task<string> NewCodeAsync()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        // Retry until the code is unique.
        for (int attempt = 0; attempt < 15; attempt++)
        {
            var suffix = new string(Enumerable.Range(0, 4).Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
            var code = $"KT-{DateTime.Now:yyMMdd}-{suffix}";
            if (!await db.Bookings.AnyAsync(b => b.BookingCode == code)) return code;
        }
        throw new InvalidOperationException("Could not generate a unique booking code.");
    }

    // Generates a unique transaction id such as TXN-yyMMdd-XXXXXXXX.
    private async Task<string> NewTxnAsync()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        // Retry until the id is unique.
        for (int attempt = 0; attempt < 15; attempt++)
        {
            var suffix = new string(Enumerable.Range(0, 8).Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
            var txn = $"TXN-{DateTime.Now:yyMMdd}-{suffix}";
            if (!await db.Bookings.AnyAsync(b => b.PaymentReference == txn)) return txn;
        }
        throw new InvalidOperationException("Could not generate a unique transaction ID.");
    }

    // Helper: truncates text to a maximum length.
    private static string Trim(string value, int max) => value.Length <= max ? value : value[..max];
}
