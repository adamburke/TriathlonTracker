using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Models;

namespace TriathlonTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Triathlon> Triathlons { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Triathlon entity
            builder.Entity<Triathlon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RaceName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired();
                entity.Property(e => e.SwimDistance).IsRequired();
                entity.Property(e => e.BikeDistance).IsRequired();
                entity.Property(e => e.RunDistance).IsRequired();
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure User entity
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
} 