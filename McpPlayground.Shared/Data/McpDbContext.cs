using McpPlayground.Shared.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace McpPlayground.Shared.Data;

public class McpDbContext : DbContext
{
    public McpDbContext(DbContextOptions<McpDbContext> options) : base(options) { }

    public DbSet<TelemetryRecord> TelemetryRecords { get; set; }
    public DbSet<CommandRecord> CommandRecords { get; set; }
    public DbSet<StatusRecord> StatusRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelemetryRecord>(entity =>
        {
            entity.ToTable("Telemetry");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DeviceId, e.Timestamp });
            entity.Property(e => e.DeviceId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DeviceName).HasMaxLength(100);
            entity.Property(e => e.OperatingMode).HasMaxLength(50);
        });

        modelBuilder.Entity<CommandRecord>(entity =>
        {
            entity.ToTable("Command");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DeviceId, e.Timestamp });
            entity.Property(e => e.DeviceId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Command).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<StatusRecord>(entity =>
        {
            entity.ToTable("Status");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DeviceId, e.Timestamp });
            entity.Property(e => e.DeviceId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DeviceName).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });
    }
}