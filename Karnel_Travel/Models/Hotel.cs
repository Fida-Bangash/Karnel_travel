
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karnel_Travel.Models
{
    // Stores hotel information
    public class Hotel
    {
        public int Id { get; set; } // Unique ID

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = ""; // Hotel name

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = ""; // Hotel location

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal Price { get; set; } // Price per night

        [Range(1, 5)]
        public int Rating { get; set; } = 1; // Hotel rating

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } // Available rooms

        public bool IsAvailable { get; set; } = true; // Booking status

        [StringLength(1000)]
        public string? Description { get; set; } // Hotel description

        [StringLength(500)]
        public string? ImagePath { get; set; } // Hotel image path
    }
}