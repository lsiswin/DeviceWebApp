using DeviceApi.Domain.Entities;
using DeviceApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeviceApi.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Device> Devices => Set<Device>();

    public DbSet<DataPoint> DataPoints => Set<DataPoint>();

    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();

    public DbSet<PlcDataRecord> PlcDataRecords => Set<PlcDataRecord>();

    public DbSet<AlarmRecord> AlarmRecords => Set<AlarmRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<AlarmRecord>(entity =>
        {
            entity.ToTable("AlarmRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Source).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Severity).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.OccurredAtUtc).IsRequired();
            entity.Property(x => x.AcknowledgedBy).HasMaxLength(80);
            entity.HasIndex(x => x.OccurredAtUtc);
            entity.HasIndex(x => x.Status);
        });
        builder.Entity<Device>(entity =>
        {
            entity.ToTable("Devices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Type).IsRequired();
            entity.Property(x => x.ProtocolType).IsRequired();
            entity.Property(x => x.ConnectionString).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        builder.Entity<DataPoint>(entity =>
        {
            entity.ToTable("DataPoints");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Address).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DataType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => new { x.DeviceId, x.Address }).IsUnique();
            entity
                .HasOne(x => x.Device)
                .WithMany(x => x.DataPoints)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OperationLog>(entity =>
        {
            entity.ToTable("OperationLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ResourceType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ResourceId).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        builder.Entity<PlcDataRecord>(entity =>
        {
            entity.ToTable("PlcDataRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DeviceId).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TagName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Value).IsRequired();
            entity.Property(x => x.ValueType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Quality).HasMaxLength(20).IsRequired().HasDefaultValue("Good");
            entity.Property(x => x.Timestamp).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();

            entity.HasIndex(x => new { x.DeviceId, x.Timestamp });
            entity.HasIndex(x => x.TagName);
            entity.HasIndex(x => x.Timestamp);

            entity
                .HasOne(x => x.Device)
                .WithMany()
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
