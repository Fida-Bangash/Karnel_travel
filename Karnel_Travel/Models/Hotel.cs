
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karnel_Travel.Models
{
    // Stores hotel information.
    public class Hotel
    {
        // Unique ID.
        public int Id { get; set; }

        // Hotel name.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        // Hotel location.
        [Required]
        [StringLength(100)]
        public string Location { get; set; } = "";

        // Price per night.
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal Price { get; set; }

        // Hotel rating.
        [Range(1, 5)]
        public int Rating { get; set; } = 1;

        // Available rooms.
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        // Booking status.
        public bool IsAvailable { get; set; } = true;

        // Hotel description.
        [StringLength(1000)]
        public string? Description { get; set; }

        // Hotel image path.
        [StringLength(500)]
        public string? ImagePath { get; set; }
    }
}