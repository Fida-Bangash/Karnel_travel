using System.Globalization;

namespace Karnel_Travel.Models
{
    // Central place for booking labels, icons, CSS classes and formatting used by views.
    public static class BookingUi
    {
        // Formats an amount, e.g. "Rs 1,234".
        public static string Money(decimal v) => "Rs " + v.ToString("#,##0.##", CultureInfo.InvariantCulture);

        // Display name for a booking type.
        public static string TypeLabel(BookingType t) => t switch
        {
            BookingType.Hotel => "Hotel",
            BookingType.Resort => "Resort",
            BookingType.Restaurant => "Restaurant",
            BookingType.Transport => "Transport",
            BookingType.Package => "Tour Package",
            BookingType.Spot => "Tourist Spot",
            _ => t.ToString()
        };

        // Font Awesome icon for a booking type.
        public static string TypeIcon(BookingType t) => t switch
        {
            BookingType.Hotel => "fa-hotel",
            BookingType.Resort => "fa-umbrella-beach",
            BookingType.Restaurant => "fa-utensils",
            BookingType.Transport => "fa-bus",
            BookingType.Package => "fa-gift",
            BookingType.Spot => "fa-mountain",
            _ => "fa-star"
        };

        // Bootstrap Icons icon for a booking type (admin panel).
        public static string TypeIconBi(BookingType t) => t switch
        {
            BookingType.Hotel => "bi-building",
            BookingType.Resort => "bi-house-door",
            BookingType.Restaurant => "bi-shop",
            BookingType.Transport => "bi-truck",
            BookingType.Package => "bi-tag",
            BookingType.Spot => "bi-geo-alt",
            _ => "bi-star"
        };

        // Display text for a booking status.
        public static string StatusLabel(BookingStatus s) => s switch
        {
            BookingStatus.Pending => "Pending",
            BookingStatus.Confirmed => "Confirmed",
            BookingStatus.Completed => "Completed",
            BookingStatus.Cancelled => "Cancelled",
            BookingStatus.Rejected => "Rejected",
            _ => s.ToString()
        };

        // CSS class for a status badge.
        public static string StatusClass(BookingStatus s) => s switch
        {
            BookingStatus.Pending => "st-pending",
            BookingStatus.Confirmed => "st-confirmed",
            BookingStatus.Completed => "st-completed",
            BookingStatus.Cancelled => "st-cancelled",
            BookingStatus.Rejected => "st-rejected",
            _ => ""
        };

        // Display text for a payment status.
        public static string PaymentLabel(PaymentStatus s) => s switch
        {
            PaymentStatus.Paid => "Paid",
            PaymentStatus.Refunded => "Refunded",
            _ => "Unpaid"
        };

        // CSS class for a payment badge.
        public static string PaymentClass(PaymentStatus s) => s switch
        {
            PaymentStatus.Paid => "pay-paid",
            PaymentStatus.Refunded => "pay-refunded",
            _ => "pay-unpaid"
        };

        // Short payment text, e.g. "Card •••• 4242" or "JazzCash".
        public static string PaymentSummary(Booking b)
        {
            // No payment recorded.
            if (string.IsNullOrWhiteSpace(b.PaymentMethod)) return "";
            // Cards show masked digits; other methods show their name.
            return b.PaymentMethod == "Card" && !string.IsNullOrWhiteSpace(b.PaymentLast4)
                ? $"Card •••• {b.PaymentLast4}"
                : b.PaymentMethod!;
        }

        // Converts "19:00" to "7:00 PM".
        public static string Slot(string? slot)
        {
            // Nothing to format.
            if (string.IsNullOrWhiteSpace(slot)) return "";
            // Format as 12-hour time; fall back to the raw value.
            return TimeSpan.TryParse(slot, CultureInfo.InvariantCulture, out var ts)
                ? DateTime.Today.Add(ts).ToString("h:mm tt", CultureInfo.InvariantCulture)
                : slot;
        }

        // One-line date summary, e.g. "12 Oct 2026 → 15 Oct 2026 (3 nights)".
        public static string When(Booking b)
        {
            // Start date text.
            var start = b.StartDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            // Stays: show the date range and number of nights.
            if (b.ItemType is BookingType.Hotel or BookingType.Resort && b.EndDate.HasValue)
                return $"{start} → {b.EndDate.Value:dd MMM yyyy} ({b.Nights} night{(b.Nights == 1 ? "" : "s")})";
            // Restaurants: show the date and time slot.
            if (b.ItemType == BookingType.Restaurant && !string.IsNullOrWhiteSpace(b.TimeSlot))
                return $"{start} · {Slot(b.TimeSlot)}";
            // Other types: start date only.
            return start;
        }

        // Quantity text per booking type, e.g. "2 rooms · 4 guests".
        public static string UnitsLabel(Booking b) => b.ItemType switch
        {
            BookingType.Hotel => $"{b.Units} room{(b.Units == 1 ? "" : "s")} · {b.Guests} guest{(b.Guests == 1 ? "" : "s")}",
            BookingType.Resort => $"{b.Units} unit{(b.Units == 1 ? "" : "s")} · {b.Guests} guest{(b.Guests == 1 ? "" : "s")}",
            BookingType.Restaurant => $"{b.Units} table{(b.Units == 1 ? "" : "s")} · {b.Guests} guest{(b.Guests == 1 ? "" : "s")}",
            BookingType.Transport => $"{b.Guests} passenger{(b.Guests == 1 ? "" : "s")}",
            _ => $"{b.Guests} person{(b.Guests == 1 ? "" : "s")}"
        };
    }
}
