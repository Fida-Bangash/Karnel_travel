
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karnel_Travel.Models
{
    // Stores transportation information.
    public class TravelInfo
    {
        // Unique ID.
        public int Id { get; set; }

        // Transport type.
        [Required]
        [StringLength(50)]
        public string TransportMode { get; set; } = "";

        // Departure city.
        [Required]
        [StringLength(100)]
        public string FromCity { get; set; } = "";

        // Destination city.
        [Required]
        [StringLength(100)]
        public string ToCity { get; set; } = "";

        // Travel fare.
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal Fare { get; set; }

        // Travel details.
        [StringLength(1000)]
        public string? Details { get; set; }
    }
}