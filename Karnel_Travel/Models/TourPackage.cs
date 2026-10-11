
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karnel_Travel.Models
{
    // Stores tour package information
    public class TourPackage
    {
        public int Id { get; set; } // Unique ID

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = ""; // Package title

        [StringLength(1000)]
        public string? Details { get; set; } // Package details

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal Price { get; set; } // Package price per person (booking isi se total banata hai)

        [Range(0, 100)]
        public int DiscountPercent { get; set; } // Discount percentage

        public DateTime? ValidTill { get; set; } // Offer expiry date

        [StringLength(500)]
        public string? ImagePath { get; set; } // Package image path
    }
}