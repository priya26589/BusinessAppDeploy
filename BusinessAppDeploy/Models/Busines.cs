using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Models
{
    public class Busines
    {
        [Key]
        public int BusinessID { get; set; } // Primary key
        public string Name { get; set; } = string.Empty; // Not nullable
        public string EmailId { get; set; } = string.Empty; // Not nullable
        public string? Password { get; set; }  // Not nullable
        public string? Description { get; set; } // Nullable
        public string? Location { get; set; } // Nullable
        public string? VisitingCard { get; set; } // Nullable
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? CategoryID { get; set; } // Not nullable
        public int SubCategoryID { get; set; } // Foreign key
        public int RoleID { get; set; }

        // Navigation property for SubCategory
        public SubCategory SubCategory { get; set; } = null!;
        // Navigation property for Role
        [ForeignKey("RoleID")]
        public Role Role { get; set; } = null!;
        public ICollection<BusinessRatings> BusinessRatings { get; set; }
    }
}
