using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Postomat.DataAccess.Database.Entities;

namespace Postomat.DataAccess.Database.Context;

public class PostomatDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public PostomatDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public PostomatDbContext(DbContextOptions<PostomatDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Cell> Cells { get; set; }
    public virtual DbSet<Log> Logs { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderPlan> OrderPlans { get; set; }
    public virtual DbSet<Entities.Postomat> Postomats { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostomatDbContext"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cell>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Cells_pkey");
            entity.HasIndex(e => e.OrderId, "fki_Cells_OrderId_fkey");
            entity.HasIndex(e => e.PostomatId, "fki_Cells_PostomatId_fkey");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.Order).WithMany(p => p.Cells)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("Cells_OrderId_fkey");
            entity.HasOne(d => d.Postomat).WithMany(p => p.Cells)
                .HasForeignKey(d => d.PostomatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Cells_PostomatId_fkey");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Logs_pkey");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Origin).HasMaxLength(256);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(256);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Orders_pkey");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ReceivingCodeHash).HasMaxLength(128);
        });

        modelBuilder.Entity<OrderPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OrderPlans_pkey");
            entity.HasIndex(e => e.CreatedBy, "fki_OrderPlans_CreatedBy_fkey");
            entity.HasIndex(e => e.DeliveredBackBy, "fki_OrderPlans_DeliveredBackBy_fkey");
            entity.HasIndex(e => e.DeliveredBy, "fki_OrderPlans_DeliveredBy_fkey");
            entity.HasIndex(e => e.OrderId, "fki_OrderPlans_OrderId_fkey");
            entity.HasIndex(e => e.PostomatId, "fki_OrderPlans_PostomatId_fkey");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DeliveryCodeHash).HasMaxLength(128);
            entity.Property(e => e.Status).HasMaxLength(128);
            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OrderPlanCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OrderPlans_CreatedBy_fkey");
            entity.HasOne(d => d.DeliveredBackByNavigation).WithMany(p => p.OrderPlanDeliveredBackByNavigations)
                .HasForeignKey(d => d.DeliveredBackBy)
                .HasConstraintName("OrderPlans_DeliveredBackBy_fkey");
            entity.HasOne(d => d.DeliveredByNavigation).WithMany(p => p.OrderPlanDeliveredByNavigations)
                .HasForeignKey(d => d.DeliveredBy)
                .HasConstraintName("OrderPlans_DeliveredBy_fkey");
            entity.HasOne(d => d.Order).WithMany(p => p.OrderPlans)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OrderPlans_OrderId_fkey");
            entity.HasOne(d => d.Postomat).WithMany(p => p.OrderPlans)
                .HasForeignKey(d => d.PostomatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OrderPlans_PostomatId_fkey");
        });

        modelBuilder.Entity<Entities.Postomat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Postomats_pkey");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Roles_pkey");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RoleName).HasMaxLength(128);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");
            entity.HasIndex(e => e.RoleId, "fki_Users_RoleId_fkey");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Login).HasMaxLength(128);
            entity.Property(e => e.PasswordHash).HasMaxLength(128);
            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Users_RoleId_fkey");
        });
    }
}