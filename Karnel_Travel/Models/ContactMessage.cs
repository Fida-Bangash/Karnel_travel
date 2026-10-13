
using System;
using System.ComponentModel.DataAnnotations;

namespace Karnel_Travel.Models
{
    // Stores customer contact messages.
    public class ContactMessage
    {
        // Unique ID.
        public int Id { get; set; }

        // Customer name.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        // Customer email.
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = "";

        // Customer phone number.
        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        // Message subject.
        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = "";

        // Message content.
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = "";

        // Submission date.
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Message read status.
        public bool IsRead { get; set; } = false;
    }
}