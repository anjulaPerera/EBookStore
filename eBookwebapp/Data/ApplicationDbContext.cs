using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using eBookwebapp.Models;
using Microsoft.AspNetCore.Identity;

namespace eBookwebapp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Review>()
                .HasKey(r => r.ReviewID);



            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
    .HasIndex(c => c.UserId)
    .IsUnique();

            // Configure the relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerID)
                .HasPrincipalKey(c => c.UserId);



        }


        public DbSet<eBookwebapp.Models.Book> Book { get; set; } = default!;
        public DbSet<eBookwebapp.Models.Customer> Customer { get; set; } = default!;
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; }
        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<eBookwebapp.Models.Order> Order { get; set; } = default!;

       

        public DbSet<eBookwebapp.Models.PaymentDetails> PaymentDetails { get; set; } = default!;

        public DbSet<OrderItem> OrderItem { get; set; }

        public DbSet<eBookwebapp.Models.Review> Reviews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-H4VU2QD\\SQLEXPRESS;Initial Catalog=eBookStorePvtLtd;Integrated Security=True; Encrypt=False", builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
            base.OnConfiguring(optionsBuilder);
        }
    }
}
