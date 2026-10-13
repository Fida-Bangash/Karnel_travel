using System.ComponentModel.DataAnnotations;

namespace Karnel_Travel.Models
{
    // Booking form input (what the user fills in).
    public class BookingInput
    {
        // Type of item being booked.
        public BookingType Type { get; set; }
        // Id of the item being booked.
        public int ItemId { get; set; }

        // Customer name (required).
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string CustomerName { get; set; } = "";

        // Customer email (required, validated).
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(254)]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; } = "";

        // Customer phone (digits, +, -, spaces and brackets).
        [Required(ErrorMessage = "Please enter your phone number.")]
        [RegularExpression(@"^[0-9+\-\s()]{7,20}$", ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone")]
        public string CustomerPhone { get; set; } = "";

        // Check-in, travel, visit or reservation date (required).
        [Required(ErrorMessage = "Please choose a date.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        // Check-out date (stays only).
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        // Restaurant time slot.
        [StringLength(10)]
        public string? TimeSlot { get; set; }

        // Number of guests.
        [Range(1, 100, ErrorMessage = "Guests must be at least 1.")]
        public int Guests { get; set; } = 1;

        // Number of rooms or tables.
        [Range(1, 20, ErrorMessage = "Please choose a valid number.")]
        public int Units { get; set; } = 1;

        // Special requests.
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // Demo payment form input. The full card number and CVV are never sent; only the last 4 digits.
    public class PaymentInput
    {
        // Payment method: Card, JazzCash or Easypaisa.
        [Required(ErrorMessage = "Please choose a payment method.")]
        public string Method { get; set; } = "Card";

        // Name on the card.
        [StringLength(60)]
        public string? CardHolder { get; set; }

        // Last 4 digits of the card (extracted in the browser).
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Enter a valid card number.")]
        public string? Last4 { get; set; }

        // Mobile wallet number (JazzCash or Easypaisa).
        [RegularExpression(@"^[0-9+\-\s()]{10,20}$", ErrorMessage = "Enter a valid mobile account number.")]
        public string? WalletNumber { get; set; }
    }

    // Price and availability result, used by the live summary and by server validation.
    public class BookingQuote
    {
        // True when the booking can proceed.
        public bool Valid { get; set; }
        // Reason when the quote is invalid.
        public string? Error { get; set; }
        // Number of nights.
        public int Nights { get; set; } = 1;
        // Rooms or tables needed.
        public int Units { get; set; } = 1;
        // Number of guests.
        public int Guests { get; set; } = 1;
        // Price per unit.
        public decimal UnitPrice { get; set; }
        // Discount percentage.
        public int DiscountPercent { get; set; }
        // Price before discount.
        public decimal Subtotal { get; set; }
        // Final price.
        public decimal Total { get; set; }
        // True when no fixed price applies.
        public bool PriceOnRequest { get; set; }
        // Rooms or tables still free (null if not tracked).
        public int? Available { get; set; }
    }

    // Item shown on the booking page: hotel, resort, restaurant, transport, package or spot in one shape.
    public class BookableItem
    {
        // Item type.
        public BookingType Type { get; set; }
        // Item id.
        public int Id { get; set; }
        // Item name.
        public string Name { get; set; } = "";
        // Item location.
        public string Location { get; set; } = "";
        // Price per room, guest or person.
        public decimal UnitPrice { get; set; }
        // Uploaded image path.
        public string? Image { get; set; }
        // Fallback image.
        public string DefaultImage { get; set; } = "/img/destination-1.jpg";
        // Item description.
        public string? Description { get; set; }
        // Star rating.
        public int Rating { get; set; }
        // Total rooms or tables (0 = inventory not tracked).
        public int Capacity { get; set; }
        // False when sold out, disabled or expired.
        public bool IsAvailable { get; set; } = true;
        // Package discount percentage.
        public int DiscountPercent { get; set; }
        // Cuisine or category.
        public string? Extra { get; set; }
        // Offer expiry (packages).
        public DateTime? ValidTill { get; set; }

        // Only hotels, resorts and restaurants track rooms or tables.
        public bool TracksInventory => Type is BookingType.Hotel or BookingType.Resort or BookingType.Restaurant;
        // Image to display, with fallback.
        public string ImageUrl => string.IsNullOrWhiteSpace(Image) ? DefaultImage : Image!;
    }

    // Booking page model: item details plus the form input.
    public class BookingFormViewModel
    {
        // Item being booked.
        public BookableItem Item { get; set; } = new();
        // Form values.
        public BookingInput Input { get; set; } = new();
    }

    // My Bookings page model: list plus status-tab counts.
    public class MyBookingsViewModel
    {
        // Bookings to list.
        public List<Booking> Bookings { get; set; } = new();
        // Selected status tab.
        public string? Status { get; set; }
        // Total bookings.
        public int All { get; set; }
        // Pending bookings.
        public int Pending { get; set; }
        // Confirmed bookings.
        public int Confirmed { get; set; }
        // Completed bookings.
        public int Done { get; set; }
        // Cancelled and rejected bookings.
        public int Closed { get; set; }

        // Ids of bookings that already have a review (hides the "Write a review" button).
        public HashSet<int> ReviewedIds { get; set; } = new();
    }

    // Review form input, filled in after a booking ends.
    public class ReviewInput
    {
        // Star rating from 1 to 5.
        [Range(1, 5, ErrorMessage = "Please choose a star rating.")]
        public int Rating { get; set; }

        // Review text (10 to 1000 characters).
        [Required(ErrorMessage = "Please write a few words about your experience.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review must be 10 to 1000 characters.")]
        public string Message { get; set; } = "";
    }

    // Review page model: the booking plus the form input.
    public class ReviewFormViewModel
    {
        // Booking being reviewed.
        public Booking Booking { get; set; } = null!;
        // Review form values.
        public ReviewInput Input { get; set; } = new();
    }

    // Admin bookings page model: filters, paging and summary counts.
    public class AdminBookingsViewModel
    {
        // Bookings on this page.
        public List<Booking> Bookings { get; set; } = new();
        // Search text.
        public string? Q { get; set; }
        // Status filter.
        public string? Status { get; set; }
        // Type filter.
        public string? Type { get; set; }
        // Start-date filter: from.
        public DateTime? From { get; set; }
        // Start-date filter: to.
        public DateTime? To { get; set; }
        // Current page.
        public int Page { get; set; } = 1;
        // Rows per page.
        public int PageSize { get; set; } = 12;
        // Total number of pages.
        public int TotalPages { get; set; } = 1;
        // Total rows matching the filters.
        public int TotalItems { get; set; }

        // All bookings.
        public int AllCount { get; set; }
        // Pending bookings.
        public int PendingCount { get; set; }
        // Confirmed bookings.
        public int ConfirmedCount { get; set; }
        // Revenue from confirmed and completed bookings.
        public decimal Revenue { get; set; }
    }
}
