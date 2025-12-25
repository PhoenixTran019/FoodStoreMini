using System;
using System.Collections.Generic;
using FoodStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodStore.Domain.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Combo> Combos { get; set; }

    public virtual DbSet<ComboDetail> ComboDetails { get; set; }

    public virtual DbSet<FoodItem> FoodItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrderTracking> OrderTrackings { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=FoodStoredb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Activity__5E5499A88BABFB65");

            entity.Property(e => e.LogId)
                .HasMaxLength(200)
                .HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(555);
            entity.Property(e => e.TagetTable).HasMaxLength(255);
            entity.Property(e => e.TargetId)
                .HasMaxLength(200)
                .HasColumnName("TargetID");
            entity.Property(e => e.TargetName).HasMaxLength(200);
            entity.Property(e => e.TimeStamp).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UserId)
                .HasMaxLength(200)
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ActivityL__UserI__5FB337D6");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B0461176C");

            entity.Property(e => e.CategoryId)
                .HasMaxLength(200)
                .HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(150);
        });

        modelBuilder.Entity<Combo>(entity =>
        {
            entity.HasKey(e => e.ComboId).HasName("PK__Combos__DD42580E80268374");

            entity.Property(e => e.ComboId)
                .HasMaxLength(200)
                .HasColumnName("ComboID");
            entity.Property(e => e.ComboName).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(555);
            entity.Property(e => e.ImageUrl)
                .IsUnicode(false)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<ComboDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__ComboDet__135C314D4916095A");

            entity.Property(e => e.DetailId)
                .HasMaxLength(200)
                .HasColumnName("DetailID");
            entity.Property(e => e.ComboId)
                .HasMaxLength(200)
                .HasColumnName("ComboID");
            entity.Property(e => e.FoodId)
                .HasMaxLength(200)
                .HasColumnName("FoodID");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Combo).WithMany(p => p.ComboDetails)
                .HasForeignKey(d => d.ComboId)
                .HasConstraintName("FK__ComboDeta__Combo__49C3F6B7");

            entity.HasOne(d => d.Food).WithMany(p => p.ComboDetails)
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("FK__ComboDeta__FoodI__4AB81AF0");
        });

        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("PK__FoodItem__856DB3CBA0879319");

            entity.Property(e => e.FoodId)
                .HasMaxLength(200)
                .HasColumnName("FoodID");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(200)
                .HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(555);
            entity.Property(e => e.FoodName).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.FoodItems)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__FoodItems__Categ__4316F928");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF4E48265E");

            entity.Property(e => e.OrderId)
                .HasMaxLength(200)
                .HasColumnName("OrderID");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(200)
                .HasColumnName("CustomerID");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(555);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ShipperId)
                .HasMaxLength(200)
                .HasColumnName("ShipperID");
            entity.Property(e => e.StatusId)
                .HasMaxLength(200)
                .HasColumnName("StatusID");
            entity.Property(e => e.TotalAmout).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderCustomers)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Orders__Customer__5070F446");

            entity.HasOne(d => d.Shipper).WithMany(p => p.OrderShippers)
                .HasForeignKey(d => d.ShipperId)
                .HasConstraintName("FK__Orders__ShipperI__5165187F");

            entity.HasOne(d => d.Status).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__Orders__StatusID__52593CB8");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30CB4E6DA24");

            entity.Property(e => e.OrderDetailId)
                .HasMaxLength(200)
                .HasColumnName("OrderDetailID");
            entity.Property(e => e.ComboId)
                .HasMaxLength(200)
                .HasColumnName("ComboID");
            entity.Property(e => e.FoodId)
                .HasMaxLength(200)
                .HasColumnName("FoodID");
            entity.Property(e => e.OrderId)
                .HasMaxLength(200)
                .HasColumnName("OrderID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Combo).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ComboId)
                .HasConstraintName("FK__OrderDeta__Combo__5812160E");

            entity.HasOne(d => d.Food).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("FK__OrderDeta__FoodI__571DF1D5");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderDeta__Order__5629CD9C");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__OrderSta__C8EE2043920DB04C");

            entity.ToTable("OrderStatus");

            entity.Property(e => e.StatusId)
                .HasMaxLength(200)
                .HasColumnName("StatusID");
            entity.Property(e => e.StatusName).HasMaxLength(155);
        });

        modelBuilder.Entity<OrderTracking>(entity =>
        {
            entity.HasKey(e => e.TrackingId).HasName("PK__OrderTra__3C19EDD165EA50F6");

            entity.ToTable("OrderTracking");

            entity.Property(e => e.TrackingId)
                .HasMaxLength(200)
                .HasColumnName("TrackingID");
            entity.Property(e => e.OrderId)
                .HasMaxLength(200)
                .HasColumnName("OrderID");
            entity.Property(e => e.StatusId)
                .HasMaxLength(200)
                .HasColumnName("StatusID");
            entity.Property(e => e.UpdateTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderTrackings)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderTrac__Order__5AEE82B9");

            entity.HasOne(d => d.Status).WithMany(p => p.OrderTrackings)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__OrderTrac__Statu__5BE2A6F2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A3B8D3E77");

            entity.Property(e => e.RoleId)
                .HasMaxLength(200)
                .HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC28D4966F");

            entity.Property(e => e.UserId)
                .HasMaxLength(200)
                .HasColumnName("UserID");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).HasMaxLength(555);
            entity.Property(e => e.RoleId)
                .HasMaxLength(200)
                .HasColumnName("RoleID");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleID__398D8EEE");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("PK__UserProf__290C8884D65C2C7D");

            entity.Property(e => e.ProfileId)
                .HasMaxLength(200)
                .HasColumnName("ProfileID");
            entity.Property(e => e.Address).HasMaxLength(555);
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.UserId)
                .HasMaxLength(200)
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.UserProfiles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserProfi__UserI__3E52440B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
