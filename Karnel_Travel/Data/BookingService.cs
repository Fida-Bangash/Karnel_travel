using System.Security.Cryptography;
using Karnel_Travel.Models;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Data;

// Booking ka poora dimagh: item dhoondna, price nikalna, availability check karna, booking banana/cancel/status badalna.
// Price aur availability HAMESHA server par nikalti hai, user ke bheje hue numbers par bharosa nahi kiya jata.
public class BookingService(Karnel_TravelContext db)
{
    // Ek room / table me zyada se zyada kitne mehman
    public const int GuestsPerUnit = 4;

    // Restaurant ke time slots (har slot ek ghanta)
    public static readonly string[] RestaurantSlots =
    {
        "12:00", "13:00", "14:00", "15:00", "18:00", "19:00", "20:00", "21:00", "22:00"
    };

    // Do log ek saath aakhri room/table book na kar saken: ek waqt me ek hi booking create hoti hai
    private static readonly SemaphoreSlim CreateGate = new(1, 1);

    // ---------------------------------------------------------------
    // 1) Item dhoondna (Hotel / Resort / Restaurant / Transport / Package / Spot)
    // ---------------------------------------------------------------
    public async Task<BookableItem?> GetItemAsync(BookingType type, int id)
    {
        switch (type)
        {
            case BookingType.Hotel:
                return await db.Hotels.AsNoTracking().Where(h => h.Id == id).Select(h => new BookableItem
                {
                    Type = type, Id = h.Id, Name = h.Name, Location = h.Location, UnitPrice = h.Price,
                    Image = h.ImagePath, DefaultImage = "/img/destination-1.jpg", Description = h.Description,
                    Rating = h.Rating, Capacity = h.Quantity, IsAvailable = h.IsAvailable && h.Quantity > 0
                }).FirstOrDefaultAsync();

            case BookingType.Resort:
                return await db.Resorts.AsNoTracking().Where(r => r.Id == id).Select(r => new BookableItem
                {
                    Type = type, Id = r.Id, Name = r.Name, Location = r.Location, UnitPrice = r.Price,
                    Image = r.ImagePath, DefaultImage = "/img/destination-2.jpg", Description = r.Description,
                    Rating = r.Rating, Capacity = r.Quantity, IsAvailable = r.IsAvailable && r.Quantity > 0
                }).FirstOrDefaultAsync();

            case BookingType.Restaurant:
                return await db.Restaurants.AsNoTracking().Where(r => r.Id == id).Select(r => new BookableItem
                {
                    Type = type, Id = r.Id, Name = r.Name, Location = r.Location, UnitPrice = r.Price,
                    Image = r.ImagePath, DefaultImage = "/img/destination-3.jpg", Description = r.Description,
                    Rating = r.Rating, Capacity = r.Quantity, IsAvailable = r.IsAvailable && r.Quantity > 0,
                    Extra = r.Cuisine
                }).FirstOrDefaultAsync();

            case BookingType.Transport:
                var t = await db.TravelInfos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (t == null) return null;
                return new BookableItem
                {
                    Type = type, Id = t.Id, Name = $"{t.TransportMode}: {t.FromCity} → {t.ToCity}",
                    Location = $"{t.FromCity} → {t.ToCity}", UnitPrice = t.Fare, DefaultImage = "/img/destination-4.jpg",
                    Description = t.Details, IsAvailable = true, Extra = t.TransportMode
                };

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
        return null;
    }

    // ---------------------------------------------------------------
    // 2) Availability: kitne rooms / tables bache hain
    // ---------------------------------------------------------------
    // Hotel/Resort: har raat ka hisab (jis raat sab se zyada rooms bhare hon, wahi limit hai)
    // Restaurant: usi din aur usi time slot ke tables
    public async Task<int> GetAvailableUnitsAsync(BookableItem item, DateTime start, DateTime? end, string? slot)
    {
        if (!item.TracksInventory) return int.MaxValue;

        var s = start.Date;

        if (item.Type == BookingType.Restaurant)
        {
            var used = await db.Bookings.AsNoTracking()
                .Where(b => b.ItemType == item.Type && b.ItemId == item.Id
                            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                            && b.StartDate == s && b.TimeSlot == slot)
                .SumAsync(b => b.Units);
            return Math.Max(0, item.Capacity - used);
        }

        var e = (end ?? s.AddDays(1)).Date;
        var rows = await db.Bookings.AsNoTracking()
            .Where(b => b.ItemType == item.Type && b.ItemId == item.Id
                        && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                        && b.StartDate < e && b.EndDate > s)
            .Select(b => new { b.StartDate, b.EndDate, b.Units })
            .ToListAsync();

        int maxUsed = 0;
        for (var d = s; d < e; d = d.AddDays(1))
        {
            int used = rows.Where(r => r.StartDate <= d && r.EndDate > d).Sum(r => r.Units);
            if (used > maxUsed) maxUsed = used;
        }
        return Math.Max(0, item.Capacity - maxUsed);
    }

    // ---------------------------------------------------------------
    // 3) Quote: input check + price + availability
    // ---------------------------------------------------------------
    public async Task<BookingQuote> QuoteAsync(BookableItem item, DateTime? start, DateTime? end, string? slot, int guests, int units)
    {
        var q = new BookingQuote { Guests = guests, UnitPrice = item.UnitPrice, DiscountPercent = item.DiscountPercent };

        if (!item.IsAvailable)
            return Fail(q, item.Type == BookingType.Package ? "This package has expired." : "This item is not available for booking right now.");

        // ---- Date
        var today = DateTime.Today;
        if (start == null) return Fail(q, "Please choose a date.");
        var s = start.Value.Date;
        if (s < today) return Fail(q, "The date cannot be in the past.");
        if (s > today.AddYears(1)) return Fail(q, "Bookings can be made up to one year in advance.");

        // ---- Type ke hisab se rules
        switch (item.Type)
        {
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

            case BookingType.Restaurant:
                if (string.IsNullOrWhiteSpace(slot) || !RestaurantSlots.Contains(slot)) return Fail(q, "Please choose a time slot.");
                if (s == today && TimeSpan.TryParse(slot, out var slotTime) && slotTime <= DateTime.Now.TimeOfDay)
                    return Fail(q, "That time slot has already passed today.");
                if (guests < 1 || guests > 40) return Fail(q, "A table reservation can have 1 to 40 guests.");
                q.Units = (int)Math.Ceiling(guests / (double)GuestsPerUnit);   // 4 mehman = 1 table
                q.Subtotal = item.UnitPrice * guests;                           // andaza: average meal x guests
                break;

            case BookingType.Transport:
                if (guests < 1 || guests > 20) return Fail(q, "You can book 1 to 20 passengers at a time.");
                q.Units = 1;
                q.Subtotal = item.UnitPrice * guests;
                break;

            case BookingType.Package:
                if (guests < 1 || guests > 50) return Fail(q, "A package can be booked for 1 to 50 persons.");
                q.Units = 1;
                q.Subtotal = item.UnitPrice * guests;
                break;

            default: // Spot
                if (guests < 1 || guests > 50) return Fail(q, "You can book for 1 to 50 persons.");
                q.Units = 1;
                q.Subtotal = 0;
                break;
        }

        // ---- Price: package par discount, jahan price nahi wahan "price on request"
        q.PriceOnRequest = item.Type == BookingType.Spot || item.UnitPrice <= 0;
        q.Total = item.Type == BookingType.Package
            ? Math.Round(q.Subtotal * (100 - item.DiscountPercent) / 100m, 2)
            : q.Subtotal;

        // ---- Availability (rooms / tables)
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

        q.Valid = true;
        return q;
    }

    private static BookingQuote Fail(BookingQuote q, string error)
    {
        q.Valid = false;
        q.Error = error;
        return q;
    }

    // ---------------------------------------------------------------
    // 4) Booking banana
    // ---------------------------------------------------------------
    public async Task<(Booking? Booking, string? Error)> CreateAsync(BookingInput input, string userId)
    {
        await CreateGate.WaitAsync();
        try
        {
            var item = await GetItemAsync(input.Type, input.ItemId);
            if (item == null) return (null, "This item no longer exists.");

            var quote = await QuoteAsync(item, input.StartDate, input.EndDate, input.TimeSlot, input.Guests, input.Units);
            if (!quote.Valid) return (null, quote.Error);

            var start = input.StartDate!.Value.Date;
            var isStay = item.Type is BookingType.Hotel or BookingType.Resort;
            var slot = item.Type == BookingType.Restaurant ? input.TimeSlot : null;

            // Spam / galti se double booking se bachao
            var duplicate = await db.Bookings.AnyAsync(b => b.UserId == userId && b.ItemType == item.Type && b.ItemId == item.Id
                && b.StartDate == start && b.TimeSlot == slot
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
            if (duplicate) return (null, "You already have an active booking for this item on this date. Check My Bookings.");

            var pending = await db.Bookings.CountAsync(b => b.UserId == userId && b.Status == BookingStatus.Pending);
            if (pending >= 5) return (null, "You already have 5 pending bookings. Please wait until they are confirmed.");

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

            db.Bookings.Add(booking);
            await db.SaveChangesAsync();
            return (booking, null);
        }
        finally
        {
            CreateGate.Release();
        }
    }

    // ---------------------------------------------------------------
    // 5) User apni booking cancel kare
    // ---------------------------------------------------------------
    public async Task<(bool Ok, string? Error)> CancelAsync(int id, string userId)
    {
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (b == null) return (false, "Booking not found.");
        if (!b.HoldsInventory) return (false, "This booking can no longer be cancelled.");
        if (b.StartDate.Date <= DateTime.Today)
            return (false, "Bookings can only be cancelled before the start date. Please contact us.");

        b.Status = BookingStatus.Cancelled;
        b.UpdatedOn = DateTime.Now;
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ---------------------------------------------------------------
    // 6) Admin status badle (Confirm / Reject / Cancel / Complete)
    // ---------------------------------------------------------------
    public static bool CanMove(BookingStatus from, BookingStatus to) => from switch
    {
        BookingStatus.Pending => to is BookingStatus.Confirmed or BookingStatus.Rejected or BookingStatus.Cancelled,
        BookingStatus.Confirmed => to is BookingStatus.Completed or BookingStatus.Cancelled,
        _ => false
    };

    public async Task<(bool Ok, string? Error)> ChangeStatusAsync(int id, BookingStatus target, string? adminNote)
    {
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return (false, "Booking not found.");
        if (!CanMove(b.Status, target))
            return (false, $"A {BookingUi.StatusLabel(b.Status).ToLower()} booking cannot be changed to {BookingUi.StatusLabel(target).ToLower()}.");
        if (target == BookingStatus.Completed && b.StartDate.Date > DateTime.Today)
            return (false, "A future booking cannot be marked as completed.");

        b.Status = target;
        if (!string.IsNullOrWhiteSpace(adminNote)) b.AdminNote = Trim(adminNote.Trim(), 500);
        b.UpdatedOn = DateTime.Now;
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------
    private async Task<string> NewCodeAsync()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";   // 0/O, 1/I jaise confusing harf nahi
        for (int attempt = 0; attempt < 15; attempt++)
        {
            var suffix = new string(Enumerable.Range(0, 4).Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
            var code = $"KT-{DateTime.Now:yyMMdd}-{suffix}";
            if (!await db.Bookings.AnyAsync(b => b.BookingCode == code)) return code;
        }
        throw new InvalidOperationException("Could not generate a unique booking code.");
    }

    private static string Trim(string value, int max) => value.Length <= max ? value : value[..max];
}
