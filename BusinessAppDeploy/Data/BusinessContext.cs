using Business.Models;
using Microsoft.EntityFrameworkCore;

namespace Business.Data
{
    public class BusinessContext : DbContext
    {
        public BusinessContext(DbContextOptions<BusinessContext> options): base(options) { }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Busines> Businesses { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<LoginRequest> loginRequests { get; set; }
        public DbSet<AdminLoginRequest> AdminLoginRequests { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<BusinessRatings> BusinessRatings { get; set; }
    }
}
