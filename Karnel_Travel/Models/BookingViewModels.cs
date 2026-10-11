using System.ComponentModel.DataAnnotations;

namespace Karnel_Travel.Models
{
    // Booking form ka input (jo user bharta hai)
    public class BookingInput
    {
        public BookingType Type { get; set; }
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(254)]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; } = "";

        [Required(ErrorMessage = "Please enter your phone number.")]
        [RegularExpression(@"^[0-9+\-\s()]{7,20}$", ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone")]
        public string CustomerPhone { get; set; } = "";

        [Required(ErrorMessage = "Please choose a date.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [StringLength(10)]
        public string? TimeSlot { get; set; }

        [Range(1, 100, ErrorMessage = "Guests must be at least 1.")]
        public int Guests { get; set; } = 1;

        [Range(1, 20, ErrorMessage = "Please choose a valid number.")]
        public int Units { get; set; } = 1;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // Price + availability ka hisab (form ki live summary aur server validation dono isi se chalte hain)
    public class BookingQuote
    {
        public bool Valid { get; set; }
        public string? Error { get; set; }
        public int Nights { get; set; } = 1;
        public int Units { get; set; } = 1;
        public int Guests { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public bool PriceOnRequest { get; set; }
        public int? Available { get; set; }
    }

    // Booking page par dikhne wali item ki jankari (Hotel/Resort/Restaurant/Transport/Package/Spot ek hi shakal me)
    public class BookableItem
    {
        public BookingType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Location { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public string? Image { get; set; }
        public string DefaultImage { get; set; } = "/img/destination-1.jpg";
        public string? Description { get; set; }
        public int Rating { get; set; }
        public int Capacity { get; set; }          // Total rooms/tables (0 = inventory track nahi hoti)
        public bool IsAvailable { get; set; } = true;
        public int DiscountPercent { get; set; }
        public string? Extra { get; set; }         // Cuisine / Category waghera
        public DateTime? ValidTill { get; set; }

        public bool TracksInventory => Type is BookingType.Hotel or BookingType.Resort or BookingType.Restaurant;
        public string ImageUrl => string.IsNullOrWhiteSpace(Image) ? DefaultImage : Image!;
    }

    public class BookingFormViewModel
    {
        public BookableItem Item { get; set; } = new();
        public BookingInput Input { get; set; } = new();
    }

    public class MyBookingsViewModel
    {
        public List<Booking> Bookings { get; set; } = new();
        public string? Status { get; set; }
        public int All { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Done { get; set; }        // Completed
        public int Closed { get; set; }      // Cancelled + Rejected
    }

    public class AdminBookingsViewModel
    {
        public List<Booking> Bookings { get; set; } = new();
        public string? Q { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; }

        public int AllCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
