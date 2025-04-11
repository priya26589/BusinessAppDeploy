using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        public string Name { get; set; } = string.Empty;

    }
}
