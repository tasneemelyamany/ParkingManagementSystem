using Microsoft.EntityFrameworkCore;
using ParkingManagementSystem.Models;

namespace ParkingManagementSystem.Data
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Site> Sites { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // site configuration
            modelBuilder.Entity<Site>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name);

                entity.HasMany(e => e.Tariffs)
                      .WithOne(t => t.Site)
                      .HasForeignKey(t => t.SiteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Tickets)
                      .WithOne(t => t.Site)
                      .HasForeignKey(t => t.SiteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // tariff configuration
            modelBuilder.Entity<Tariff>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.SiteId);
            });

            // ticket configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.PlateNumber);
                entity.HasIndex(e => e.SiteId);
                entity.HasIndex(e => new { e.PlateNumber, e.To }); // For finding active tickets
                entity.HasIndex(e => e.From);
                entity.HasIndex(e => e.To);

                // Configure UTC adaption
                entity.Property(e => e.From)
                      .HasConversion(
                          v => v,
                          v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                entity.Property(e => e.To)
                      .HasConversion(
                          v => v,
                          v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            });
        }
    }
}
