
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karnel_Travel.Models
{
    // Stores resort information.
    public class Resort
    {
        // Unique ID.
        public int Id { get; set; }

        // Resort name.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        // Resort location.
        [Required]
        [StringLength(100)]
        public string Location { get; set; } = "";

        // Resort price.
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal Price { get; set; }

        // Resort rating.
        [Range(1, 5)]
        public int Rating { get; set; } = 1;

        // Available rooms or units.
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        // Availability status.
        public bool IsAvailable { get; set; } = true;

        // Resort description.
        [StringLength(1000)]
        public string? Description { get; set; }

        // Resort image path.
        [StringLength(500)]
        public string? ImagePath { get; set; }
    }
}