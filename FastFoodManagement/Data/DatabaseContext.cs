using System;
using System.Data.Entity;
using FastFoodManagement.Data.Models;
using FastFoodManagement.Expenses.Models;

namespace FastFoodManagement.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=FastFoodConnection")
        {
        }

        // Original models
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        
        // New models
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure relationships and constraints here
            
            // Order and OrderItems relationship
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithRequired()
                .HasForeignKey(oi => oi.OrderId)
                .WillCascadeOnDelete(true);
            
            // Product and Category relationship
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .WillCascadeOnDelete(false);
            
            // Expense configuration
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Expense>()
                .HasIndex(e => e.Date);
            
            base.OnModelCreating(modelBuilder);
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