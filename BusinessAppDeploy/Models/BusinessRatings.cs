using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class BusinessRatings
    {
        [Key]
        public int BusinessRatingID { get; set; } // Primary key

        [Required]
        public int BusinessID { get; set; } // Foreign key

        [ForeignKey("BusinessID")]
        public virtual Busines? Business { get; set; } // Navigation property

        [Required]
        [Column(TypeName = "decimal(3,2)")] // Ensures rating is stored properly
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        public decimal Rating { get; set; }

        [Required]
        [EmailAddress]
        public string RatedBy { get; set; } = string.Empty;

        public string? Comment { get; set; } // Optional

        [Required]
        public DateTime DateTime { get; set; } = DateTime.UtcNow; // Set default value
    }
}
