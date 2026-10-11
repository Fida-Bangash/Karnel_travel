
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karnel_Travel.Models
{
    // Stores transportation information
    public class TravelInfo
    {
        public int Id { get; set; } // Unique ID

        [Required]
        [StringLength(50)]
        public string TransportMode { get; set; } = ""; // Transport type

        [Required]
        [StringLength(100)]
        public string FromCity { get; set; } = ""; // Departure city

        [Required]
        [StringLength(100)]
        public string ToCity { get; set; } = ""; // Destination city

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal Fare { get; set; } // Travel fare

        [StringLength(1000)]
        public string? Details { get; set; } // Travel details
    }
}