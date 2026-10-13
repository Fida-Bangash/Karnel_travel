
using System.ComponentModel.DataAnnotations;

namespace Karnel_Travel.Models
{
    // Stores tourist spot information.
    public class TouristSpot
    {
        // Unique ID.
        public int Id { get; set; }

        // Spot name.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        // Spot category.
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "";

        // Spot location.
        [Required]
        [StringLength(100)]
        public string Location { get; set; } = "";

        // Spot description.
        [StringLength(1000)]
        public string? Description { get; set; }

        // Spot image path.
        [StringLength(500)]
        public string? ImagePath { get; set; }
    }
}