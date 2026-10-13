
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Data
{
    
    // EF Core database context: Identity tables plus application tables.
    public class Karnel_TravelContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor: options (connection string) come from Program.cs.
        public Karnel_TravelContext(
            DbContextOptions<Karnel_TravelContext> options)
            : base(options)
        {
        }

        // Configures indexes and relationships.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Keep the default Identity configuration.
            base.OnModelCreating(builder);

            // Feedback: one review per booking (old feedback without a booking is ignored).
            builder.Entity<Feedback>(f =>
            {
                f.HasIndex(x => x.BookingId).IsUnique().HasFilter("[BookingId] IS NOT NULL");
                f.HasIndex(x => x.CreatedOn);
            });

            // Booking: unique code, plus indexes for availability, user and status lookups.
            builder.Entity<Booking>(b =>
            {
                b.HasIndex(x => x.BookingCode).IsUnique();
                b.HasIndex(x => new { x.ItemType, x.ItemId, x.StartDate });
                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.Status);

                // Keep the booking if its user is deleted (UserId is set to null).
                b.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }

        // Tourist spots.
        public DbSet<TouristSpot> TouristSpots { get; set; }
        // Transport and travel information.
        public DbSet<TravelInfo> TravelInfos { get; set; }
        // Hotels.
        public DbSet<Hotel> Hotels { get; set; }
        // Restaurants.
        public DbSet<Restaurant> Restaurants { get; set; }
        // Resorts.
        public DbSet<Resort> Resorts { get; set; }
        // Tour packages and discounts.
        public DbSet<TourPackage> TourPackages { get; set; }
        // Customer messages and orders.
        public DbSet<ContactMessage> ContactMessages { get; set; }
        // Customer feedback and reviews.
        public DbSet<Feedback> Feedbacks { get; set; }
        // Bookings for hotels, resorts, restaurants, transport, packages and spots.
        public DbSet<Booking> Bookings { get; set; }
    }
}
