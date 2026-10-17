using Microsoft.EntityFrameworkCore;
using RoadIncidentApi.Models;

namespace RoadIncidentApi.Data;

/// <summary>Kontekst EF Core odwzorowujący domenę na bazę SQLite.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<IncidentReport> Reports => Set<IncidentReport>();
    public DbSet<Reporter> Reporters => Set<Reporter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reporter>(e =>
        {
            e.Property(p => p.FullName).IsRequired().HasMaxLength(120);
            e.Property(p => p.Email).IsRequired().HasMaxLength(200);
            e.Property(p => p.PhoneNumber).HasMaxLength(30);
        });

        modelBuilder.Entity<IncidentReport>(e =>
        {
            e.Property(p => p.Title).IsRequired().HasMaxLength(150);
            e.Property(p => p.Description).HasMaxLength(2000);
            e.Property(p => p.City).IsRequired().HasMaxLength(100);
            e.Property(p => p.Street).HasMaxLength(150);

            // Relacja 1:N — jeden zgłaszający, wiele zgłoszeń.
            e.HasOne(p => p.Reporter)
                .WithMany(r => r.Reports)
                .HasForeignKey(p => p.ReporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
