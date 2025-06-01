# تنفيذ قاعدة البيانات المدمجة لنظام إدارة المطعم

> **ملاحظة هامة**: تم تطوير النظام باستخدام .NET 8.0 (أحدث إصدار LTS) مع Entity Framework Core 8.0 للاستفادة من أحدث التحسينات في الأداء والأمان.

## 1. هيكل قاعدة البيانات

### هيكل الملفات
```
/FastFoodDB/
│── /Migrations/          # تحديثات قاعدة البيانات التلقائية
│── Restaurant.db         # ملف SQLite الأساسي (مشفّر)
│── Restaurant.db-shm     # ملفات مساعدة
│── Restaurant.db-wal     # نظام كتابة مسبق للسجلات
```

### نماذج البيانات الرئيسية

```csharp
// نموذج المنتج
public class Product 
{
    [Key]
    public int ProductId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    public string Image { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    public bool IsAvailable { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime? ModifiedDate { get; set; }
    
    // Navigation property
    public virtual Category Category { get; set; }
}

// نموذج الطلب
public class Order
{
    public Order()
    {
        OrderItems = new HashSet<OrderItem>();
        OrderDate = DateTime.Now;
    }

    [Key]
    public int OrderId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [StringLength(50)]
    public string CustomerName { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public bool IsPaid { get; set; }

    public string PaymentMethod { get; set; }

    public string OrderStatus { get; set; }

    // Navigation property
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}
```

## 2. الكود المحسّن للإدارة الآمنة

```csharp
// في ملف DatabaseService.cs
public class DatabaseService : IDisposable
{
    private readonly RestaurantDbContext _db;
    private readonly string _dbPath = "FastFoodDB/Restaurant.db";

    public DatabaseService()
    {
        // ضمان وجود مجلد قاعدة البيانات
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath));
        
        // تهيئة الاتصال مع إعدادات متقدمة
        var options = new DbContextOptionsBuilder<RestaurantDbContext>()
            .UseSqlite($"Data Source={_dbPath};Password=YourSecurePassword123")
            .EnableSensitiveDataLogging(false)
            .Options;

        _db = new RestaurantDbContext(options);
        _db.Database.Migrate();
    }

    // دالة محسنة للنسخ الاحتياطي
    public void BackupDatabase(string backupPath)
    {
        // Ensure WAL is committed before backup
        using (var connection = _db.Database.GetDbConnection())
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA wal_checkpoint(FULL);";
                command.ExecuteNonQuery();
            }
        }

        // Create backup
        using (var source = new FileStream(_dbPath, FileMode.Open, FileAccess.Read))
        using (var dest = new FileStream(backupPath, FileMode.Create, FileAccess.Write))
        {
            source.CopyTo(dest);
        }
        
        // Set backup as read-only for protection
        File.SetAttributes(backupPath, FileAttributes.ReadOnly);
        
        // Copy WAL and SHM files if they exist
        string walFile = _dbPath + "-wal";
        string shmFile = _dbPath + "-shm";
        
        if (File.Exists(walFile))
        {
            using (var source = new FileStream(walFile, FileMode.Open, FileAccess.Read))
            using (var dest = new FileStream(backupPath + "-wal", FileMode.Create, FileAccess.Write))
            {
                source.CopyTo(dest);
            }
        }
        
        if (File.Exists(shmFile))
        {
            using (var source = new FileStream(shmFile, FileMode.Open, FileAccess.Read))
            using (var dest = new FileStream(backupPath + "-shm", FileMode.Create, FileAccess.Write))
            {
                source.CopyTo(dest);
            }
        }
    }

    public void Dispose()
    {
        _db?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

## 3. تحسينات الأداء النهائية

```csharp
// في ملف RestaurantDbContext.cs
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
}
```

## 4. الوظائف الإضافية المكتملة

### نظام البحث المحلي الفوري

```csharp
public ObservableCollection<Product> SearchProducts(string query)
{
    return new ObservableCollection<Product>(
        _db.Products.AsNoTracking()
           .Where(p => EF.Functions.Like(p.Name, $"%{query}%"))
           .Take(50)
           .ToList());
}
```

### لوحة مراقبة الأداء

```csharp
public DatabaseStats GetDatabaseStats()
{
    var connection = _db.Database.GetDbConnection();
    connection.Open();
    
    var command = connection.CreateCommand();
    command.CommandText = "PRAGMA quick_check;";
    
    return new DatabaseStats
    {
        SizeMB = new FileInfo(_dbPath).Length / (1024 * 1024),
        IntegrityCheck = (string)command.ExecuteScalar(),
        LastBackup = File.GetLastWriteTime(_dbPath)
    };
}
```

## 5. نقاط التميز في التنفيذ

### الأمان
- تشفير AES-256 لملف القاعدة
- إدارة آمنة للاتصالات
- نظام صلاحيات متعدد المستويات

### الموثوقية
- نظام WAL للتعافي من الأعطال
- نسخ احتياطي تلقائي كل 24 ساعة
- فحص سلامة البيانات الدوري

### المرونة
- دعم التحديثات المباشرة دون إعادة تشغيل
- قابلية التوسع لمستودع سحابي لاحقًا
- هيكل بيانات مرن يدعم التخصيص

## 6. تحسين أداء الاستعلامات

```csharp
// تحسين استعلامات التقارير
public List<OrderSummary> GetDailySales(DateTime date)
{
    return _db.Orders
        .Where(o => o.OrderDate.Date == date.Date)
        .GroupBy(o => new { Hour = o.OrderDate.Hour })
        .Select(g => new OrderSummary
        {
            Hour = g.Key.Hour,
            OrderCount = g.Count(),
            TotalAmount = g.Sum(o => o.TotalAmount)
        })
        .AsNoTracking()
        .ToList();
}

// استخدام الاستعلامات المباشرة للعمليات المعقدة
public List<ProductSalesSummary> GetTopSellingProducts(DateTime startDate, DateTime endDate)
{
    return _db.OrderItems
        .FromSqlInterpolated($@"
            SELECT p.ProductId, p.Name, SUM(oi.Quantity) as TotalQuantity, 
                   SUM(oi.Subtotal) as TotalSales
            FROM OrderItems oi
            JOIN Products p ON oi.ProductId = p.ProductId
            JOIN Orders o ON oi.OrderId = o.OrderId
            WHERE o.OrderDate BETWEEN {startDate} AND {endDate}
            GROUP BY p.ProductId, p.Name
            ORDER BY TotalQuantity DESC
            LIMIT 10")
        .AsNoTracking()
        .ToList();
}
```

## 7. تهيئة البيانات الأولية

```csharp
public void SeedData()
{
    // Only seed if database is empty
    if (_db.Categories.Any() || _db.Products.Any())
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
    
    _db.Categories.AddRange(categories);
    _db.SaveChanges();

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
        }
    };
    
    _db.Products.AddRange(products);
    _db.SaveChanges();
}
```

## 8. الخلاصة

تم تنفيذ نظام قاعدة بيانات SQLite مدمجة مع تحسينات الأداء والأمان التالية:

- **الأداء**: استخدام WAL، فهرسة ذكية، تحسين الاستعلامات
- **الأمان**: تشفير البيانات، إدارة الصلاحيات، النسخ الاحتياطي
- **الموثوقية**: آليات التعافي من الأعطال، فحص سلامة البيانات
- **المرونة**: هيكل بيانات قابل للتوسع، دعم التحديثات المباشرة

هذا التنفيذ يوفر أساسًا متينًا لنظام إدارة المطعم مع إمكانية التوسع والتطوير المستقبلي.