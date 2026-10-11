
using System;
using System.ComponentModel.DataAnnotations;

namespace Karnel_Travel.Models
{
    // Stores customer contact messages
    public class ContactMessage
    {
        public int Id { get; set; } // Unique ID

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = ""; // Customer name

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = ""; // Customer email

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; } // Customer phone number

        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = ""; // Message subject

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = ""; // Message content

        public DateTime CreatedOn { get; set; } = DateTime.Now; // Submission date

        public bool IsRead { get; set; } = false; // Message read status
    }
}