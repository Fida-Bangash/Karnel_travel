using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Karnel_Travel.Data;

namespace Karnel_Travel.Models
{
    // Kind of item being booked.
    public enum BookingType
    {
        Hotel = 1,
        Resort = 2,
        Restaurant = 3,
        Transport = 4,
        Package = 5,
        Spot = 6
    }

    // Booking lifecycle: Pending, Confirmed, Completed, or Cancelled / Rejected.
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Completed = 2,
        Cancelled = 3,
        Rejected = 4
    }

    // Payment state (demo payments; no real money is charged).
    public enum PaymentStatus
    {
        Unpaid = 0,
        Paid = 1,
        Refunded = 2
    }

    // A customer booking or reservation (full record).
    public class Booking
    {
        // Primary key.
        public int Id { get; set; }

        // Unique customer-facing code, e.g. KT-261010-A7K2.
        [Required, StringLength(20)]
        public string BookingCode { get; set; } = "";

        // Owner of the booking (login required).
        public string? UserId { get; set; }
        // Navigation property to the owner.
        public ApplicationUser? User { get; set; }

        // Type of the booked item.
        public BookingType ItemType { get; set; }
        // Id of the item in its own table.
        public int ItemId { get; set; }

        // Name and location are copied at booking time, so the record stays correct if the item changes or is deleted.
        [Required, StringLength(150)]
        public string ItemName { get; set; } = "";

        // Location snapshot.
        [StringLength(200)]
        public string? ItemLocation { get; set; }

        // Customer name.
        [Required, StringLength(100)]
        public string CustomerName { get; set; } = "";

        // Customer email.
        [Required, StringLength(254)]
        public string CustomerEmail { get; set; } = "";

        // Customer phone.
        [Required, StringLength(20)]
        public string CustomerPhone { get; set; } = "";

        // Check-in, travel, visit or reservation date.
        public DateTime StartDate { get; set; }

        // Check-out date (hotels and resorts only).
        public DateTime? EndDate { get; set; }

        // Restaurant time slot, e.g. "19:00".
        [StringLength(10)]
        public string? TimeSlot { get; set; }

        // Number of guests, passengers or persons.
        public int Guests { get; set; } = 1;

        // Rooms or tables held by this booking.
        public int Units { get; set; } = 1;

        // Number of nights (hotels and resorts); 1 otherwise.
        public int Nights { get; set; } = 1;

        // Price per unit at booking time.
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Discount applied (packages).
        public int DiscountPercent { get; set; }

        // Final total calculated on the server.
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Customer special requests.
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Current booking status.
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Admin note visible to the customer (e.g. rejection reason).
        [StringLength(500)]
        public string? AdminNote { get; set; }

        // Creation time.
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        // Last update time.
        public DateTime? UpdatedOn { get; set; }

        // Payment (demo): current payment state.
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        // Card, JazzCash or Easypaisa.
        [StringLength(20)]
        public string? PaymentMethod { get; set; }

        // Demo transaction id, e.g. TXN-261012-8F3K2A91.
        [StringLength(30)]
        public string? PaymentReference { get; set; }

        // Last 4 card digits only; the full number is never stored.
        [StringLength(4)]
        public string? PaymentLast4 { get; set; }

        // Payment time.
        public DateTime? PaidOn { get; set; }

        // True for priced bookings that need payment (not restaurants or spots).
        [NotMapped]
        public bool NeedsPayment => TotalPrice > 0 && ItemType != BookingType.Restaurant && ItemType != BookingType.Spot;

        // True when payment is possible: payable, unpaid and still active.
        [NotMapped]
        public bool CanPayNow => NeedsPayment && PaymentStatus == PaymentStatus.Unpaid && HoldsInventory;

        // Users may cancel only pending bookings before the start date; confirmed ones go through support.
        [NotMapped]
        public bool CanUserCancel => Status == BookingStatus.Pending && StartDate.Date > DateTime.Today;

        // Last day of the booking: check-out for stays, otherwise the start date.
        [NotMapped]
        public DateTime LastDay => (EndDate ?? StartDate).Date;

        // A review opens after the last day, for confirmed or completed bookings only.
        [NotMapped]
        public bool CanReviewNow =>
            (Status == BookingStatus.Confirmed || Status == BookingStatus.Completed) && LastDay < DateTime.Today;

        // Pending and confirmed bookings hold rooms or tables.
        [NotMapped]
        public bool HoldsInventory => Status == BookingStatus.Pending || Status == BookingStatus.Confirmed;
    }
}
