using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Models;

namespace TemplateJwtProject.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Models.Route> Routes { get; set; }
    public DbSet<RouteLocation> RouteLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // RefreshToken configuratie
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Entity<Building>()
            .Property(b => b.Name)
            .HasMaxLength(100);

        builder.Entity<Building>()
            .Property(b => b.Description)
            .HasMaxLength(500);

        builder.Entity<Building>()
            .HasIndex(b => b.Name)
            .IsUnique();

        builder.Entity<Location>()
            .Property(l => l.Name)
            .HasMaxLength(100);

        builder.Entity<Location>()
            .Property(l => l.Description)
            .HasMaxLength(1000);

        builder.Entity<Location>()
            .HasIndex(l => l.Name);

        builder.Entity<Location>()
            .HasOne(l => l.Building)
            .WithMany(b => b.Locations)
            .HasForeignKey(l => l.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Models.Route>()
            .Property(r => r.Name)
            .HasMaxLength(100);

        builder.Entity<Models.Route>()
            .Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Entity<Models.Route>()
            .HasIndex(r => r.Name)
            .IsUnique();

        builder.Entity<RouteLocation>()
            .HasKey(rl => new { rl.RouteId, rl.LocationId });

        builder.Entity<RouteLocation>()
            .Property(rl => rl.Notes)
            .HasMaxLength(500);

        builder.Entity<RouteLocation>()
            .HasIndex(rl => new { rl.RouteId, rl.Order })
            .IsUnique();

        builder.Entity<RouteLocation>()
            .HasOne(rl => rl.Route)
            .WithMany(r => r.RouteLocations)
            .HasForeignKey(rl => rl.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RouteLocation>()
            .HasOne(rl => rl.Location)
            .WithMany(l => l.RouteLocations)
            .HasForeignKey(rl => rl.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Building>().HasData(
            new Building { Id = 1, Name = "Hoofdgebouw", Description = "Centraal schoolgebouw met receptie en kantoren." },
            new Building { Id = 2, Name = "Techniekgebouw", Description = "Gebouw met technieklokalen en praktijkruimtes." }
        );

        builder.Entity<Location>().HasData(
            new Location
            {
                Id = 1,
                Name = "Receptie",
                Description = "Hoofdreceptie voor bezoekers en studenten.",
                Floor = 0,
                XCoordinate = 12.5,
                YCoordinate = 8.2,
                BuildingId = 1
            },
            new Location
            {
                Id = 2,
                Name = "Aula",
                Description = "Grote ontmoetingsruimte voor evenementen en pauzes.",
                Floor = 0,
                XCoordinate = 24.1,
                YCoordinate = 14.7,
                BuildingId = 1
            },
            new Location
            {
                Id = 3,
                Name = "Praktijklokaal T1",
                Description = "Praktijklokaal voor technieklessen.",
                Floor = 1,
                XCoordinate = 7.4,
                YCoordinate = 29.3,
                BuildingId = 2
            }
        );

        builder.Entity<Models.Route>().HasData(
            new Models.Route
            {
                Id = 1,
                Name = "Open Dag Route",
                Description = "Route langs de belangrijkste plekken voor bezoekers.",
                EstimatedTimeMinutes = 20
            },
            new Models.Route
            {
                Id = 2,
                Name = "Techniek Route",
                Description = "Route door de techniekvleugel en praktijklokalen.",
                EstimatedTimeMinutes = 15
            }
        );

        builder.Entity<RouteLocation>().HasData(
            new RouteLocation { RouteId = 1, LocationId = 1, Order = 1, Notes = "Start bij de ingang." },
            new RouteLocation { RouteId = 1, LocationId = 2, Order = 2, Notes = "Loop via de centrale hal." },
            new RouteLocation { RouteId = 2, LocationId = 1, Order = 1, Notes = "Startpunt voor bezoekers." },
            new RouteLocation { RouteId = 2, LocationId = 3, Order = 2, Notes = "Eindpunt in het techniekgebouw." }
        );
    }
}
