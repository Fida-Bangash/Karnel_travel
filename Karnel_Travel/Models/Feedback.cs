
using System;
using System.ComponentModel.DataAnnotations;

namespace Karnel_Travel.Models
{
    // Stores customer feedback and booking reviews.
    public class Feedback
    {
        // Unique ID.
        public int Id { get; set; }

        // User name.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        // User email.
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = "";

        // Feedback message.
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = "";

        // Submission date.
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Id of the reviewing user (empty for old feedback).
        [StringLength(450)]
        public string? UserId { get; set; }

        // Reviewed booking; one review per booking (empty for old feedback).
        public int? BookingId { get; set; }

        // Type of the booked item, shown with the review.
        public BookingType? ItemType { get; set; }

        // Name of the booked item, shown with the review.
        [StringLength(150)]
        public string? ItemName { get; set; }

        // Star rating from 1 to 5 (0 = no rating, old feedback).
        [Range(0, 5)]
        public int Rating { get; set; }
    }
}