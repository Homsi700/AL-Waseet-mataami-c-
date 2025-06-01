using System;
using System.Collections.Generic;
using System.Linq;
using FastFoodManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FastFoodManagement.Data
{
    public class DatabaseInitializer
    {
        private readonly RestaurantDbContext _context;

        public DatabaseInitializer(RestaurantDbContext context)
        {
            _context = context;
        }

        public void Initialize()
        {
            // Ensure database is created and migrated
            _context.Database.Migrate();
            
            // Seed data if needed
            SeedData();
        }

        private void SeedData()
        {
            // Only seed if database is empty
            if (_context.Categories.Any() || _context.Products.Any())
            {
                return; // Database already has data
            }

            // Seed categories
            var categories = new List<Category>
            {
                new Category { Name = "وجبات رئيسية", Description = "الأطباق الرئيسية" },
                new Category { Name = "مشروبات", Description = "المشروبات الباردة والساخنة" },
                new Category { Name = "حلويات", Description = "الحلويات والتحلية" },
                new Category { Name = "مقبلات", Description = "المقبلات والسلطات" },
                new Category { Name = "وجبات جانبية", Description = "الأطباق الجانبية" }
            };
            
            _context.Categories.AddRange(categories);
            _context.SaveChanges();

            // Seed products
            var products = new List<Product>
            {
                // وجبات رئيسية
                new Product 
                { 
                    Name = "برجر لحم", 
                    Description = "برجر لحم مع جبنة وخضروات", 
                    Price = 25.00m, 
                    CategoryId = categories[0].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "برجر دجاج", 
                    Description = "برجر دجاج مع صلصة خاصة", 
                    Price = 22.00m, 
                    CategoryId = categories[0].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "شاورما لحم", 
                    Description = "شاورما لحم مع صلصة طحينة", 
                    Price = 28.00m, 
                    CategoryId = categories[0].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "شاورما دجاج", 
                    Description = "شاورما دجاج مع صلصة ثوم", 
                    Price = 24.00m, 
                    CategoryId = categories[0].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                
                // مشروبات
                new Product 
                { 
                    Name = "كولا", 
                    Description = "مشروب غازي", 
                    Price = 5.00m, 
                    CategoryId = categories[1].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "عصير برتقال", 
                    Description = "عصير برتقال طازج", 
                    Price = 8.00m, 
                    CategoryId = categories[1].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "شاي", 
                    Description = "شاي ساخن", 
                    Price = 4.00m, 
                    CategoryId = categories[1].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                
                // حلويات
                new Product 
                { 
                    Name = "كنافة", 
                    Description = "كنافة بالجبنة", 
                    Price = 15.00m, 
                    CategoryId = categories[2].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "بقلاوة", 
                    Description = "بقلاوة بالفستق", 
                    Price = 12.00m, 
                    CategoryId = categories[2].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                
                // مقبلات
                new Product 
                { 
                    Name = "حمص", 
                    Description = "حمص مع زيت زيتون", 
                    Price = 10.00m, 
                    CategoryId = categories[3].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "متبل", 
                    Description = "متبل باذنجان", 
                    Price = 10.00m, 
                    CategoryId = categories[3].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "سلطة", 
                    Description = "سلطة خضار طازجة", 
                    Price = 8.00m, 
                    CategoryId = categories[3].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                
                // وجبات جانبية
                new Product 
                { 
                    Name = "بطاطس مقلية", 
                    Description = "بطاطس مقلية مقرمشة", 
                    Price = 10.00m, 
                    CategoryId = categories[4].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                },
                new Product 
                { 
                    Name = "حلقات بصل", 
                    Description = "حلقات بصل مقلية", 
                    Price = 12.00m, 
                    CategoryId = categories[4].CategoryId,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now
                }
            };
            
            _context.Products.AddRange(products);
            
            // Seed default user
            var defaultUser = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // In a real app, use a secure password hashing library
                FullName = "مدير النظام",
                Email = "admin@example.com",
                Role = "Admin",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            
            _context.Users.Add(defaultUser);
            
            // Seed default settings
            var defaultSettings = new List<AppSetting>
            {
                new AppSetting 
                { 
                    SettingKey = "CompanyName", 
                    SettingValue = "مطعم الوسيط", 
                    SettingGroup = "General", 
                    Description = "اسم المطعم" 
                },
                new AppSetting 
                { 
                    SettingKey = "CompanyAddress", 
                    SettingValue = "شارع الرئيسي، المدينة", 
                    SettingGroup = "General", 
                    Description = "عنوان المطعم" 
                },
                new AppSetting 
                { 
                    SettingKey = "CompanyPhone", 
                    SettingValue = "0123456789", 
                    SettingGroup = "General", 
                    Description = "رقم هاتف المطعم" 
                },
                new AppSetting 
                { 
                    SettingKey = "TaxRate", 
                    SettingValue = "0.15", 
                    SettingGroup = "Financial", 
                    Description = "نسبة الضريبة" 
                },
                new AppSetting 
                { 
                    SettingKey = "CurrencySymbol", 
                    SettingValue = "ر.س", 
                    SettingGroup = "Financial", 
                    Description = "رمز العملة" 
                },
                new AppSetting 
                { 
                    SettingKey = "ReceiptFooter", 
                    SettingValue = "شكراً لزيارتكم! نتمنى لكم وجبة شهية.", 
                    SettingGroup = "Printing", 
                    Description = "نص تذييل الفاتورة" 
                },
                new AppSetting 
                { 
                    SettingKey = "DefaultPrinter", 
                    SettingValue = "", 
                    SettingGroup = "Printing", 
                    Description = "الطابعة الافتراضية" 
                },
                new AppSetting 
                { 
                    SettingKey = "BackupPath", 
                    SettingValue = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FastFoodDB", "Backups"), 
                    SettingGroup = "System", 
                    Description = "مسار النسخ الاحتياطي" 
                },
                new AppSetting 
                { 
                    SettingKey = "AutoBackup", 
                    SettingValue = "true", 
                    SettingGroup = "System", 
                    Description = "تمكين النسخ الاحتياطي التلقائي" 
                }
            };
            
            _context.AppSettings.AddRange(defaultSettings);
            
            _context.SaveChanges();
        }
    }
}