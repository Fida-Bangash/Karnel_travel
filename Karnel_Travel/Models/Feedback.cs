
using System;
using System.ComponentModel.DataAnnotations;

namespace Karnel_Travel.Models
{
    // Stores user feedback
    public class Feedback
    {
        public int Id { get; set; } // Unique ID

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = ""; // User name

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = ""; // User email

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = ""; // Feedback message

        public DateTime CreatedOn { get; set; } = DateTime.Now; // Submission date
    }
}