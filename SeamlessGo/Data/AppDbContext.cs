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

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<StockLocation> StockLocation { get; set; }
        public DbSet<Plan> Plans { get; set; } 
        public DbSet<Sequence> Sequences { get; set; }


        public DbSet<SeamlessGo.Models.Route> Routes { get; set; }

       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

       
            modelBuilder.Entity<User>()
                .HasOne(u => u.client)
                .WithMany()
                .HasForeignKey(u => u.ClientID);

         
            modelBuilder.Entity<User>()
                .HasOne(u => u.stockLocation) 
                .WithOne() 
                .HasForeignKey<StockLocation>(sl => sl.DriverUserID) // 
                .OnDelete(DeleteBehavior.Restrict);

            // Route entity configuration
            modelBuilder.Entity<SeamlessGo.Models.Route>(entity =>
            {
                entity.HasKey(e => e.RouteID);
                entity.ToTable("Routes");
                entity.HasOne(r => r.Plan)
     .WithMany() // or .WithMany(p => p.Routes) if Plan has collection
     .HasForeignKey(r => r.PlanID)
     .OnDelete(DeleteBehavior.Restrict); 
            });

            // StockLocation entity configuration
            modelBuilder.Entity<StockLocation>(entity =>
            {
                entity.HasKey(e => e.LocationID);
                entity.ToTable("StockLocations");
            });

            modelBuilder.Entity<Sequence>(entity =>
            {
                entity.HasKey(e => new { e.UserID, e.TableName });

                entity.ToTable("Sequences"); 

                entity.Property(e => e.TableName)
                    .HasMaxLength(255);

                entity.Property(e => e.TablePrefix)
                    .HasMaxLength(50);
            });
        }
    }
}
