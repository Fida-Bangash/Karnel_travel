
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Data
{
    // Poore project ka database context: Identity (login/roles) ke saath saare travel tables bhi yahan hain
    public class Karnel_TravelContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor: connection string Program.cs se options ke zariye aati hai
        public Karnel_TravelContext(
            DbContextOptions<Karnel_TravelContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Booking>(b =>
            {
                b.HasIndex(x => x.BookingCode).IsUnique();
                b.HasIndex(x => new { x.ItemType, x.ItemId, x.StartDate });
                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.Status);

                // User delete ho jaye to booking record rahe (UserId null ho jata hai)
                b.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }

        public DbSet<TouristSpot> TouristSpots { get; set; }      // Tourist spots ka table
        public DbSet<TravelInfo> TravelInfos { get; set; }        // Transport / travel information ka table
        public DbSet<Hotel> Hotels { get; set; }                  // Hotels ka table
        public DbSet<Restaurant> Restaurants { get; set; }        // Restaurants ka table
        public DbSet<Resort> Resorts { get; set; }                // Resorts ka table
        public DbSet<TourPackage> TourPackages { get; set; }      // Packages aur discounts ka table
        public DbSet<ContactMessage> ContactMessages { get; set; } // Customer ke messages / orders
        public DbSet<Feedback> Feedbacks { get; set; }            // Customer feedback
        public DbSet<Booking> Bookings { get; set; }              // Hotel/Resort/Restaurant/Transport/Package/Spot bookings
    }
}
