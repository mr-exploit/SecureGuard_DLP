using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Models;

namespace SecureGuard.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Models.Agent> Agents { get; set; }
    public DbSet<LogEntry> Logs { get; set; }
    public DbSet<Models.Alert> Alerts { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<WhitelistEntry> WhitelistEntries { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Agent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.Hostname);
            entity.HasMany(e => e.Logs).WithOne(l => l.Agent).HasForeignKey(l => l.AgentId).IsRequired(false);
            entity.HasMany(e => e.Alerts).WithOne(a => a.Agent).HasForeignKey(a => a.AgentId).IsRequired(false);
        });

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.ViolationType);
        });

        modelBuilder.Entity<Models.Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.Acknowledged);
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<WhitelistEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IpAddress).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Seed default admin user
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
            Email = "admin@secureguard.local",
            Role = "admin",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        });

        // Seed default whitelist entries
        modelBuilder.Entity<WhitelistEntry>().HasData(
            new WhitelistEntry { Id = 1, IpAddress = "127.0.0.1", Description = "Localhost", AddedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new WhitelistEntry { Id = 2, IpAddress = "::1", Description = "IPv6 Localhost", AddedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed default policies
        modelBuilder.Entity<Policy>().HasData(
            new Policy { Id = 1, Name = "Block Image Uploads", RuleType = "IMAGE_UPLOAD", Pattern = "image/*", Action = "block", Severity = "HIGH", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Policy { Id = 2, Name = "Block Credential Files", RuleType = "CREDENTIAL_FILE", Pattern = ".env,secrets.json,credentials.json", Action = "block", Severity = "CRITICAL", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Policy { Id = 3, Name = "Flag Unknown IPs", RuleType = "UNKNOWN_IP", Pattern = "*", Action = "flag", Severity = "MEDIUM", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Policy { Id = 4, Name = "Block Credential Patterns", RuleType = "CREDENTIAL_PATTERN", Pattern = "AWS_SECRET_ACCESS_KEY|API_KEY|DATABASE_PASSWORD|JWT_SECRET", Action = "block", Severity = "CRITICAL", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
