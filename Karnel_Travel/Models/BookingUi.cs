using System.Globalization;

namespace Karnel_Travel.Models
{
    // Views me booking ke labels, icons aur rang ek jagah se milte hain
    public static class BookingUi
    {
        public static string Money(decimal v) => "Rs " + v.ToString("#,##0.##", CultureInfo.InvariantCulture);

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

        // Admin panel (Bootstrap Icons) ke liye
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

        public static string StatusLabel(BookingStatus s) => s switch
        {
            BookingStatus.Pending => "Pending",
            BookingStatus.Confirmed => "Confirmed",
            BookingStatus.Completed => "Completed",
            BookingStatus.Cancelled => "Cancelled",
            BookingStatus.Rejected => "Rejected",
            _ => s.ToString()
        };

        public static string StatusClass(BookingStatus s) => s switch
        {
            BookingStatus.Pending => "st-pending",
            BookingStatus.Confirmed => "st-confirmed",
            BookingStatus.Completed => "st-completed",
            BookingStatus.Cancelled => "st-cancelled",
            BookingStatus.Rejected => "st-rejected",
            _ => ""
        };

        // "19:00" -> "7:00 PM"
        public static string Slot(string? slot)
        {
            if (string.IsNullOrWhiteSpace(slot)) return "";
            return TimeSpan.TryParse(slot, CultureInfo.InvariantCulture, out var ts)
                ? DateTime.Today.Add(ts).ToString("h:mm tt", CultureInfo.InvariantCulture)
                : slot;
        }

        // "12 Oct 2026 -> 15 Oct 2026 (3 nights)" jaisa chhota summary
        public static string When(Booking b)
        {
            var start = b.StartDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            if (b.ItemType is BookingType.Hotel or BookingType.Resort && b.EndDate.HasValue)
                return $"{start} → {b.EndDate.Value:dd MMM yyyy} ({b.Nights} night{(b.Nights == 1 ? "" : "s")})";
            if (b.ItemType == BookingType.Restaurant && !string.IsNullOrWhiteSpace(b.TimeSlot))
                return $"{start} · {Slot(b.TimeSlot)}";
            return start;
        }

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
