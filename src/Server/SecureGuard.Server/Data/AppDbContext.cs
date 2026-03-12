using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Models;

namespace SecureGuard.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<LogEntry> LogEntries => Set<LogEntry>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<WhitelistEntry> WhitelistEntries => Set<WhitelistEntry>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasIndex(e => e.AgentId).IsUnique();
            entity.HasIndex(e => e.IsOnline);
            entity.HasIndex(e => e.LastSeen);
        });

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.ViolationType);
            entity.HasIndex(e => e.Severity);
            entity.HasOne(e => e.AgentEntity)
                  .WithMany(a => a.Logs)
                  .HasForeignKey(e => e.AgentEntityId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.IsResolved);
            entity.HasOne(e => e.AgentEntity)
                  .WithMany(a => a.Alerts)
                  .HasForeignKey(e => e.AgentEntityId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WhitelistEntry>(entity =>
        {
            entity.HasIndex(e => e.IpAddress).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        if (!db.Users.Any())
        {
            var adminEmail = config["AdminUser:Email"] ?? "admin@secureguard.local";
            var adminPassword = config["AdminUser:Password"] ?? "Admin@SecureGuard2024!";

            db.Users.Add(new User
            {
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = "Admin",
                FullName = "System Administrator",
                IsActive = true
            });
        }

        if (!db.Policies.Any())
        {
            db.Policies.AddRange(
                new Policy
                {
                    Name = "Block Image Upload",
                    Description = "Block all image file uploads via HTTP/HTTPS",
                    RuleType = "ImageUpload",
                    Action = "Block",
                    IsEnabled = true,
                    Parameters = "{\"contentTypes\": [\"image/*\"]}"
                },
                new Policy
                {
                    Name = "Block Credential Files",
                    Description = "Block upload of credential files (.env, app.config, secrets.json, credentials.json)",
                    RuleType = "CredentialFile",
                    Action = "Block",
                    IsEnabled = true,
                    Parameters = "{\"extensions\": [\".env\", \".config\", \"secrets.json\", \"credentials.json\"]}"
                },
                new Policy
                {
                    Name = "Block Credential Patterns",
                    Description = "Block content containing credential patterns like AWS keys, API keys, etc.",
                    RuleType = "CredentialPattern",
                    Action = "Block",
                    IsEnabled = true,
                    Parameters = "{\"patterns\": [\"AWS_SECRET_ACCESS_KEY\", \"API_KEY\", \"DATABASE_PASSWORD\", \"JWT_SECRET\"]}"
                },
                new Policy
                {
                    Name = "Flag Unknown IP",
                    Description = "Flag connections to IPs not in the whitelist",
                    RuleType = "UnknownIp",
                    Action = "Flag",
                    IsEnabled = true,
                    Parameters = "{}"
                }
            );
        }

        if (!db.WhitelistEntries.Any())
        {
            db.WhitelistEntries.AddRange(
                new WhitelistEntry { IpAddress = "127.0.0.1", Description = "Localhost", CreatedBy = "system" },
                new WhitelistEntry { IpAddress = "10.0.0.0/8", Description = "Private network 10.x.x.x", CreatedBy = "system" },
                new WhitelistEntry { IpAddress = "192.168.0.0/16", Description = "Private network 192.168.x.x", CreatedBy = "system" },
                new WhitelistEntry { IpAddress = "172.16.0.0/12", Description = "Private network 172.16-31.x.x", CreatedBy = "system" }
            );
        }

        await db.SaveChangesAsync();
    }
}
