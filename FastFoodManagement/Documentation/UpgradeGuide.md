# دليل الترقية من .NET 6.0 إلى .NET 8.0

## مقدمة

هذا الدليل يوضح خطوات ترقية نظام إدارة المطعم من .NET 6.0 إلى .NET 8.0، مع شرح التغييرات الرئيسية والفوائد المتوقعة من هذه الترقية.

## متطلبات الترقية

### 1. متطلبات النظام

- **نظام التشغيل**: Windows 10 (إصدار 1809 أو أحدث) أو Windows 11
- **المساحة المطلوبة**: 2 جيجابايت على الأقل
- **الذاكرة**: 4 جيجابايت RAM على الأقل
- **.NET SDK**: تثبيت .NET SDK 8.0 أو أحدث

### 2. البرامج المطلوبة

- Visual Studio 2022 (الإصدار 17.8 أو أحدث)
- أو Visual Studio Code مع C# Dev Kit
- Git (اختياري، للتحكم في الإصدارات)

## خطوات الترقية

### 1. تحديث ملفات المشروع

#### 1.1 تحديث ملف المشروع (.csproj)

```xml
<!-- قبل الترقية -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0-windows</TargetFramework>
    <!-- ... -->
  </PropertyGroup>
</Project>

<!-- بعد الترقية -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <!-- ... -->
  </PropertyGroup>
</Project>
```

#### 1.2 تحديث حزم NuGet

```xml
<!-- قبل الترقية -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.10" />

<!-- بعد الترقية -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
```

### 2. تعديلات الكود

#### 2.1 تحديثات Entity Framework Core

```csharp
// قبل الترقية
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>()
        .HasMany(o => o.Items)
        .WithOne(i => i.Order);
}

// بعد الترقية - استخدام ميزات EF Core 8.0
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>()
        .HasMany(o => o.Items)
        .WithOne(i => i.Order)
        .IsRequired()
        .HasForeignKey(i => i.OrderId)
        .OnDelete(DeleteBehavior.Cascade);
        
    // استخدام ميزة جديدة: HasTrigger
    modelBuilder.Entity<Order>()
        .ToTable(tb => tb.HasTrigger("UpdateInventoryTrigger"));
}
```

#### 2.2 استخدام ميزات C# 12

```csharp
// قبل الترقية
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    public Product(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}

// بعد الترقية - استخدام Primary Constructor
public class Product(int id, string name, decimal price)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public decimal Price { get; set; } = price;
}
```

#### 2.3 استخدام Collection Expressions

```csharp
// قبل الترقية
var categories = new List<string> { "وجبات رئيسية", "مشروبات", "حلويات" };

// بعد الترقية
var categories = ["وجبات رئيسية", "مشروبات", "حلويات"];
```

### 3. تحديث قاعدة البيانات

#### 3.1 إنشاء ترحيل جديد (Migration)

```bash
# في سطر الأوامر
dotnet ef migrations add UpgradeToNet8
dotnet ef database update
```

#### 3.2 تحديث الاتصال بقاعدة البيانات

```csharp
// قبل الترقية
services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlite("Data Source=restaurant.db"));

// بعد الترقية - استخدام ميزات SQLite المحسنة
services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlite("Data Source=restaurant.db", sqliteOptions =>
    {
        sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }));
```

### 4. تحديث واجهة المستخدم

#### 4.1 تحديث XAML

```xml
<!-- قبل الترقية -->
<Button Content="حفظ" Background="Blue" Foreground="White" />

<!-- بعد الترقية - استخدام ميزات WPF المحسنة -->
<Button Content="حفظ" 
        CornerRadius="8"
        Background="{DynamicResource PrimaryBrush}"
        Foreground="White"/>
```

## التغييرات المتوافقة مع الإصدارات السابقة

### 1. تغييرات قد تتطلب تعديلات

- تغييرات في سلوك `DateTime` و `DateTimeOffset`
- تغييرات في سلوك `JsonSerializer`
- تغييرات في سلوك Entity Framework Core

### 2. كيفية التعامل مع التغييرات

```csharp
// مثال على التعامل مع تغييرات DateTime
// قبل الترقية
var date = DateTime.Parse(dateString);

// بعد الترقية - تحديد ثقافة صريحة
var date = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
```

## اختبار الترقية

### 1. اختبارات الوحدة

```csharp
// تحديث اختبارات الوحدة لاستخدام ميزات MSTest الجديدة
[TestClass]
public class OrderServiceTests
{
    [TestMethod]
    public async Task CreateOrder_ShouldSaveOrderCorrectly()
    {
        // استخدام ميزات اختبار جديدة في .NET 8.0
        await using var context = new TestDbContext();
        var service = new OrderService(context);
        
        var result = await service.CreateOrderAsync(new Order());
        
        Assert.IsNotNull(result);
        Assert.AreEqual(1, await context.Orders.CountAsync());
    }
}
```

### 2. اختبارات التكامل

```bash
# تشغيل اختبارات التكامل
dotnet test --filter Category=Integration
```

## فوائد الترقية

### 1. تحسينات الأداء

- تحسين وقت تنفيذ العمليات بنسبة تصل إلى 33%
- تقليل استهلاك الذاكرة
- تحسين وقت بدء التشغيل

### 2. ميزات جديدة

- دعم أفضل للتشفير والأمان
- تحسينات في Entity Framework Core
- ميزات لغة C# 12 الجديدة

### 3. الدعم طويل الأمد

- دعم .NET 8.0 حتى نوفمبر 2026
- تحديثات أمنية منتظمة
- إصلاحات للأخطاء

## استكشاف الأخطاء وإصلاحها

### 1. مشاكل شائعة

#### مشكلة: خطأ في تحميل المكتبات

```
System.IO.FileNotFoundException: Could not load file or assembly 'Microsoft.EntityFrameworkCore, Version=8.0.0.0'
```

**الحل**: تأكد من تحديث جميع حزم NuGet المرتبطة بـ Entity Framework Core إلى الإصدار 8.0.0 أو أحدث.

#### مشكلة: أخطاء في الترجمة

```
error CS0518: Predefined type 'System.Runtime.CompilerServices.RequiredMemberAttribute' is not defined or imported
```

**الحل**: تأكد من تثبيت .NET SDK 8.0 وتحديث ملف المشروع لاستخدام `<TargetFramework>net8.0-windows</TargetFramework>`.

### 2. موارد إضافية

- [وثائق ترقية .NET 8.0 الرسمية](https://docs.microsoft.com/dotnet/core/compatibility/8.0)
- [تغييرات Entity Framework Core 8.0](https://docs.microsoft.com/ef/core/what-is-new/ef-core-8.0/breaking-changes)
- [ميزات C# 12 الجديدة](https://docs.microsoft.com/dotnet/csharp/whats-new/csharp-12)

## الخلاصة

ترقية نظام إدارة المطعم من .NET 6.0 إلى .NET 8.0 توفر العديد من الفوائد من حيث الأداء والأمان والميزات الجديدة. باتباع هذا الدليل، يمكن إجراء الترقية بسلاسة والاستفادة من أحدث التحسينات في منصة .NET.

## الخطوات التالية

1. إجراء اختبارات شاملة بعد الترقية
2. تحديث الوثائق الفنية
3. تدريب فريق التطوير على ميزات .NET 8.0 الجديدة
4. التخطيط للاستفادة من ميزات .NET 8.0 المتقدمة في التطويرات المستقبلية