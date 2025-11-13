using SeamlessGo.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;


namespace SeamlessGo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 👇 These map your models to database tables
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; } // Organization = Client
        public DbSet<StockLocation> StockLocation { get; set; } // Organization = Client

        public DbSet<SeamlessGo.Models.Route> Routes { get; set; }

        // Optional: override OnModelCreating to configure relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User to Client relationship (one-to-many: one client can have many users)
            modelBuilder.Entity<User>()
                .HasOne(u => u.client)
                .WithMany()
                .HasForeignKey(u => u.ClientID);

            // User to StockLocation relationship (one-to-one)
            // ✅ StockLocation has the FK (DriverUserID) pointing to User
            modelBuilder.Entity<User>()
                .HasOne(u => u.stockLocation) // ✅ User has one StockLocation
                .WithOne() // ✅ StockLocation has one User (no navigation property in StockLocation)
                .HasForeignKey<StockLocation>(sl => sl.DriverUserID) // ✅ FK is in StockLocation table
                .OnDelete(DeleteBehavior.Restrict);

            // Route entity configuration
            modelBuilder.Entity<SeamlessGo.Models.Route>(entity =>
            {
                entity.HasKey(e => e.RouteID);
                entity.ToTable("Routes");
            });

            // StockLocation entity configuration
            modelBuilder.Entity<StockLocation>(entity =>
            {
                entity.HasKey(e => e.LocationID);
                entity.ToTable("StockLocations");
            });
        }
    }
}
