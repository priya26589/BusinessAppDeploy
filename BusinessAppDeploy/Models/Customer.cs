using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Models
{
    public class Customer
    {
        [Key]
        public int Cus_Id { get; set; }
        public string? Cus_EmailId { get; set; } = string.Empty; // Not nullable
        public string? Cus_Password { get; set; }
        public string? Cus_Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        // Foreign key property
        public int RoleID { get; set; }
        // Navigation property
        [ForeignKey("RoleID")]
        public Role Role { get; set; } = null!;
        public int? PinCode { get; set; }
    }
}
