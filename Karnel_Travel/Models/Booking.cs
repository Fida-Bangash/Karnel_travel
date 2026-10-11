using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Karnel_Travel.Data;

namespace Karnel_Travel.Models
{
    // Booking kis cheez ki hai (Hotel, Resort, Restaurant, Transport, Package ya Tourist Spot)
    public enum BookingType
    {
        Hotel = 1,
        Resort = 2,
        Restaurant = 3,
        Transport = 4,
        Package = 5,
        Spot = 6
    }

    // Booking ki halat: Pending -> Confirmed -> Completed (ya Cancelled / Rejected)
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Completed = 2,
        Cancelled = 3,
        Rejected = 4
    }

    // Customer ki booking / reservation ka poora record
    public class Booking
    {
        public int Id { get; set; }

        // Customer ko dikhane wala unique code, jaise KT-261010-A7K2
        [Required, StringLength(20)]
        public string BookingCode { get; set; } = "";

        // Booking karne wale user ki ID (login zaroori hai)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public BookingType ItemType { get; set; }
        public int ItemId { get; set; }

        // Booking ke waqt ka naam/location (baad me item badal jaye ya delete ho tab bhi record sahi rahe)
        [Required, StringLength(150)]
        public string ItemName { get; set; } = "";

        [StringLength(200)]
        public string? ItemLocation { get; set; }

        [Required, StringLength(100)]
        public string CustomerName { get; set; } = "";

        [Required, StringLength(254)]
        public string CustomerEmail { get; set; } = "";

        [Required, StringLength(20)]
        public string CustomerPhone { get; set; } = "";

        // Check-in / travel date / visit date / reservation date
        public DateTime StartDate { get; set; }

        // Check-out date (sirf Hotel / Resort ke liye)
        public DateTime? EndDate { get; set; }

        // Restaurant ka time slot, jaise "19:00"
        [StringLength(10)]
        public string? TimeSlot { get; set; }

        // Mehmanon / passengers / persons ki tadaad
        public int Guests { get; set; } = 1;

        // Rooms / tables ki tadaad (inventory jo is booking ne roki hui hai)
        public int Units { get; set; } = 1;

        // Raaton ki tadaad (Hotel / Resort), baaki me 1
        public int Nights { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int DiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Admin ka note jo customer ko bhi dikhta hai (jaise reject ki wajah)
        [StringLength(500)]
        public string? AdminNote { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; set; }

        // Pending ya Confirmed booking inventory (rooms/tables) roke rakhti hai
        [NotMapped]
        public bool HoldsInventory => Status == BookingStatus.Pending || Status == BookingStatus.Confirmed;
    }
}
