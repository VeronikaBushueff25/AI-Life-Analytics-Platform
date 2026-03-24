using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AILifeAnalytics.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Insight> Insights { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }
    public DbSet<PersonalityProfile> PersonalityProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Role)
             .HasConversion<string>()  
             .HasMaxLength(20);
        });

        mb.Entity<Activity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.Date }).IsUnique(); 
            e.HasOne(x => x.User)
             .WithMany(x => x.Activities)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.Property(x => x.MoodReason).HasMaxLength(500);
            e.Property(x => x.HighlightsOfDay).HasMaxLength(500);
        });

        mb.Entity<Insight>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithMany(x => x.Insights)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.AnalysisType)
             .HasConversion<string>()
             .HasMaxLength(20);
            e.Property(x => x.Content).HasMaxLength(2000);
        });

        mb.Entity<UserSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasOne(x => x.User)
             .WithOne(x => x.Settings)
             .HasForeignKey<UserSettings>(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.ActiveProvider)
             .HasConversion<string>()
             .HasMaxLength(20);
        });

        mb.Entity<PersonalityProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.GeneratedAt });
            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.ArchetypeName).HasMaxLength(100);
            e.Property(x => x.FullAnalysis).HasMaxLength(5000);
        });
    }
}