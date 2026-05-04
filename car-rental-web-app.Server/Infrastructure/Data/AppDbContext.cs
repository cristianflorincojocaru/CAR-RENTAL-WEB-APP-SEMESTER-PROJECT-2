using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SecurityAlert> SecurityAlerts => Set<SecurityAlert>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── User ───────────────────────────────────────────────
        mb.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).UseIdentityColumn();
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            e.Property(u => u.Phone).HasMaxLength(20);
            e.Property(u => u.Role).HasConversion<int>();

            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Role);
            e.HasIndex(u => u.BranchId);

            e.HasOne(u => u.Branch)
             .WithMany(b => b.Staff)
             .HasForeignKey(u => u.BranchId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);

            e.HasOne(u => u.CreatedByUser)
             .WithMany()
             .HasForeignKey(u => u.CreatedByUserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── Branch ─────────────────────────────────────────────
        mb.Entity<Branch>(e =>
        {
            e.ToTable("Branches");
            e.HasKey(b => b.Id);
            e.Property(b => b.Id).UseIdentityColumn();
            e.Property(b => b.Name).HasMaxLength(100).IsRequired();
            e.Property(b => b.City).HasMaxLength(100).IsRequired();
            e.Property(b => b.Address).HasMaxLength(300).IsRequired();
            e.Property(b => b.Phone).HasMaxLength(20).IsRequired();

            e.HasIndex(b => b.Name);
            e.HasIndex(b => b.City);

            e.HasOne(b => b.Manager)
             .WithMany()
             .HasForeignKey(b => b.ManagerId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── Vehicle ────────────────────────────────────────────
        mb.Entity<Vehicle>(e =>
        {
            e.ToTable("Vehicles");
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).UseIdentityColumn();
            e.Property(v => v.RegistrationNumber).HasMaxLength(20).IsRequired();
            e.Property(v => v.Brand).HasMaxLength(100).IsRequired();
            e.Property(v => v.Model).HasMaxLength(100).IsRequired();
            e.Property(v => v.Category).HasConversion<int>();
            e.Property(v => v.Status).HasConversion<int>();
            e.Property(v => v.DailyRate).HasColumnType("decimal(10,2)");
            e.Property(v => v.Rating).HasColumnType("decimal(3,2)");
            e.Property(v => v.FuelType).HasMaxLength(30);
            e.Property(v => v.Transmission).HasMaxLength(20);
            e.Property(v => v.ColorHex).HasMaxLength(10);

            e.HasIndex(v => v.RegistrationNumber).IsUnique();
            e.HasIndex(v => v.Category);
            e.HasIndex(v => v.BranchId);
            e.HasIndex(v => v.Status);

            e.HasOne(v => v.Branch)
             .WithMany(b => b.Vehicles)
             .HasForeignKey(v => v.BranchId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.AddedByUser)
             .WithMany(u => u.AddedVehicles)
             .HasForeignKey(v => v.AddedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Client ─────────────────────────────────────────────
        mb.Entity<Client>(e =>
        {
            e.ToTable("Clients");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).UseIdentityColumn();
            e.Property(c => c.FullName).HasMaxLength(150).IsRequired();
            e.Property(c => c.Email).HasMaxLength(200).IsRequired();
            e.Property(c => c.Phone).HasMaxLength(20).IsRequired();
            e.Property(c => c.Address).HasMaxLength(300);
            e.Property(c => c.FlagReason).HasMaxLength(500);

            e.HasIndex(c => c.Email).IsUnique();
            e.HasIndex(c => c.Phone).IsUnique();
            e.HasIndex(c => c.FullName);
        });

        // ── Rental ─────────────────────────────────────────────
        mb.Entity<Rental>(e =>
        {
            e.ToTable("Rentals");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).UseIdentityColumn();
            e.Property(r => r.BookingReference).HasMaxLength(20).IsRequired();
            e.Property(r => r.TotalCost).HasColumnType("decimal(10,2)");
            e.Property(r => r.Status).HasConversion<int>();
            e.Property(r => r.PickupLocation).HasMaxLength(200).IsRequired();
            e.Property(r => r.ReturnLocation).HasMaxLength(200).IsRequired();
            e.Property(r => r.CancellationReason).HasMaxLength(500);
            e.Property(r => r.ProtectionPlan).HasMaxLength(50);
            e.Property(r => r.Extras).HasMaxLength(500);
            e.Property(r => r.Notes).HasMaxLength(1000);
            e.Property(r => r.RowVersion).IsRowVersion();

            e.HasIndex(r => r.BookingReference).IsUnique();
            e.HasIndex(r => r.VehicleId);
            e.HasIndex(r => r.ClientId);
            e.HasIndex(r => r.BranchId);
            e.HasIndex(r => r.Status);
            e.HasIndex(r => new { r.StartDate, r.EndDate });

            e.HasOne(r => r.Vehicle)
             .WithMany(v => v.Rentals)
             .HasForeignKey(r => r.VehicleId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Client)
             .WithMany(c => c.Rentals)
             .HasForeignKey(r => r.ClientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Branch)
             .WithMany(b => b.Rentals)
             .HasForeignKey(r => r.BranchId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.CreatedByUser)
             .WithMany(u => u.CreatedRentals)
             .HasForeignKey(r => r.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.CompletedByUser)
             .WithMany(u => u.CompletedRentals)
             .HasForeignKey(r => r.CompletedByUserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── RefreshToken ────────────────────────────────────────
        mb.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshTokens");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).UseIdentityColumn();
            e.Property(t => t.Token).HasMaxLength(500).IsRequired();

            e.HasIndex(t => t.Token).IsUnique();
            e.HasIndex(t => t.UserId);

            e.HasOne(t => t.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLog ────────────────────────────────────────────
        mb.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).UseIdentityColumn();
            e.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            e.Property(a => a.Action).HasMaxLength(100).IsRequired();
            e.Property(a => a.Details).HasMaxLength(1000).IsRequired();

            e.HasIndex(a => new { a.EntityType, a.EntityId });
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => a.UserId);

            e.HasOne(a => a.User)
             .WithMany(u => u.AuditLogs)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── SecurityAlert ───────────────────────────────────────
        mb.Entity<SecurityAlert>(e =>
        {
            e.ToTable("SecurityAlerts");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).UseIdentityColumn();
            e.Property(s => s.AlertType).HasConversion<int>();
            e.Property(s => s.Description).HasMaxLength(500).IsRequired();

            e.HasIndex(s => s.UserId);
            e.HasIndex(s => s.IsRead);

            e.HasOne(s => s.User)
             .WithMany(u => u.SecurityAlerts)
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
