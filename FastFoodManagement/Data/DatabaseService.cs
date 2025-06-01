using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FastFoodManagement.Data.Models;

namespace FastFoodManagement.Data
{
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

        // Restore database from backup
        public bool RestoreDatabase(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("Backup file not found", backupPath);
                }
                
                // Close current database connection
                _db.Dispose();
                
                // Remove read-only attribute if present
                if ((File.GetAttributes(backupPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(backupPath, File.GetAttributes(backupPath) & ~FileAttributes.ReadOnly);
                }
                
                // Restore database file
                using (var source = new FileStream(backupPath, FileMode.Open, FileAccess.Read))
                using (var dest = new FileStream(_dbPath, FileMode.Create, FileAccess.Write))
                {
                    source.CopyTo(dest);
                }
                
                // Restore WAL and SHM files if they exist
                string walBackup = backupPath + "-wal";
                string shmBackup = backupPath + "-shm";
                
                if (File.Exists(walBackup))
                {
                    using (var source = new FileStream(walBackup, FileMode.Open, FileAccess.Read))
                    using (var dest = new FileStream(_dbPath + "-wal", FileMode.Create, FileAccess.Write))
                    {
                        source.CopyTo(dest);
                    }
                }
                
                if (File.Exists(shmBackup))
                {
                    using (var source = new FileStream(shmBackup, FileMode.Open, FileAccess.Read))
                    using (var dest = new FileStream(_dbPath + "-shm", FileMode.Create, FileAccess.Write))
                    {
                        source.CopyTo(dest);
                    }
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // نظام البحث المحلي الفوري
        public ObservableCollection<Product> SearchProducts(string query)
        {
            return new ObservableCollection<Product>(
                _db.Products.AsNoTracking()
                   .Where(p => EF.Functions.Like(p.Name, $"%{query}%"))
                   .Take(50)
                   .ToList());
        }

        // لوحة مراقبة الأداء
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

        // Optimize database
        public void OptimizeDatabase()
        {
            var connection = _db.Database.GetDbConnection();
            connection.Open();
            
            using (var command = connection.CreateCommand())
            {
                // Vacuum the database to reclaim space
                command.CommandText = "VACUUM;";
                command.ExecuteNonQuery();
                
                // Analyze to update statistics
                command.CommandText = "ANALYZE;";
                command.ExecuteNonQuery();
                
                // Optimize indexes
                command.CommandText = "PRAGMA optimize;";
                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    // Database statistics class
    public class DatabaseStats
    {
        public double SizeMB { get; set; }
        public string IntegrityCheck { get; set; }
        public DateTime LastBackup { get; set; }
    }
}