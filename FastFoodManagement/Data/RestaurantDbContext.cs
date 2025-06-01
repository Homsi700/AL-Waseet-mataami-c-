using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using FastFoodManagement.Data.Models;
using FastFoodManagement.Expenses.Models;

namespace FastFoodManagement.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
        }

        // Database sets
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // فهرسة الحقول الشائعة الاستخدام
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.Name, p.CategoryId })
                .IsClustered(false);

            // تكوين العلاقات مع حذف متدرج
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // تحسين تخزين التواريخ
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Local));

            // Optimize Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
                entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
            });

            // Optimize Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
                
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Optimize OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(oi => oi.Subtotal).HasColumnType("decimal(18,2)");
                
                entity.HasOne(oi => oi.Product)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Optimize Expense entity
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasIndex(e => e.Date);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                
                entity.Property(e => e.Date)
                      .HasConversion(
                          v => v.ToUniversalTime(),
                          v => DateTime.SpecifyKind(v, DateTimeKind.Local));
            });

            // Optimize User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // Optimize AppSetting entity
            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.HasIndex(s => s.SettingKey).IsUnique();
                entity.Property(s => s.SettingKey).IsRequired().HasMaxLength(50);
                entity.Property(s => s.SettingValue).IsRequired();
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default configuration if not already configured
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FastFoodDB", "Restaurant.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
            
            // Enable Write-Ahead Logging for better performance and crash recovery
            optionsBuilder.EnableSensitiveDataLogging(false);
            
            base.OnConfiguring(optionsBuilder);
        }
    }

    // User model for authentication
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
    
    // Application settings model
    public class AppSetting
    {
        public int Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string SettingGroup { get; set; }
        public string Description { get; set; }
    }
}