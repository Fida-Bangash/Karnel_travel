
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karnel_Travel.Models
{
    // Stores restaurant information.
    public class Restaurant
    {
        // Unique ID.
        public int Id { get; set; }

        // Restaurant name.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        // Restaurant location.
        [Required]
        [StringLength(100)]
        public string Location { get; set; } = "";

        // Food type.
        [StringLength(100)]
        public string? Cuisine { get; set; }

        // Average meal price.
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal Price { get; set; }

        // Restaurant rating.
        [Range(1, 5)]
        public int Rating { get; set; } = 1;

        // Available tables or units.
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        // Availability status.
        public bool IsAvailable { get; set; } = true;

        // Restaurant description.
        [StringLength(1000)]
        public string? Description { get; set; }

        // Restaurant image path.
        [StringLength(500)]
        public string? ImagePath { get; set; }
    }
}