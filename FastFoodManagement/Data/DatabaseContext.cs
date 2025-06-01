using System;
using System.Data.Entity;
using FastFoodManagement.Data.Models;

namespace FastFoodManagement.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=FastFoodConnection")
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure relationships and constraints here
            base.OnModelCreating(modelBuilder);
        }
    }
}