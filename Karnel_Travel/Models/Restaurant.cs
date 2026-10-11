
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karnel_Travel.Models
{
    // Stores restaurant information
    public class Restaurant
    {
        public int Id { get; set; } // Unique ID

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = ""; // Restaurant name

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = ""; // Restaurant location

        [StringLength(100)]
        public string? Cuisine { get; set; } // Food type

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal Price { get; set; } // Average meal price

        [Range(1, 5)]
        public int Rating { get; set; } = 1; // Restaurant rating

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } // Available tables or units

        public bool IsAvailable { get; set; } = true; // Availability status

        [StringLength(1000)]
        public string? Description { get; set; } // Restaurant description

        [StringLength(500)]
        public string? ImagePath { get; set; } // Restaurant image path
    }
}