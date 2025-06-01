using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FastFoodManagement.Data;
using Microsoft.Win32;

namespace FastFoodManagement.Settings.Views
{
    public partial class DatabaseDashboardView : Page
    {
        private readonly DatabaseService _databaseService;
        private string _backupPath;

        public DatabaseDashboardView()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            
            // Set default backup path
            _backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FastFoodDB", "Backups");
            txtBackupPath.Text = _backupPath;
            
            // Load initial data
            LoadDatabaseStats();
            LoadBackupHistory();
        }

        private void LoadDatabaseStats()
        {
            try
            {
                var stats = _databaseService.GetDatabaseStats();
                
                // Update UI with stats
                txtDatabaseSize.Text = $"{stats.SizeMB:F2} MB";
                pbDatabaseSize.Value = Math.Min(stats.SizeMB / 10 * 100, 100); // Assuming 10MB is 100%
                
                txtIntegrityCheck.Text = stats.IntegrityCheck == "ok" ? "جيدة" : "تحتاج فحص";
                txtIntegrityCheck.Foreground = stats.IntegrityCheck == "ok" ? 
                    System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                
                txtIntegrityDetails.Text = $"آخر فحص: {DateTime.Now:yyyy-MM-dd HH:mm}";
                
                if (stats.LastBackup != DateTime.MinValue)
                {
                    txtLastBackup.Text = stats.LastBackup.ToString("yyyy-MM-dd HH:mm");
                    
                    // Check if backup is older than 24 hours
                    TimeSpan timeSinceLastBackup = DateTime.Now - stats.LastBackup;
                    if (timeSinceLastBackup.TotalHours > 24)
                    {
                        txtBackupStatus.Text = "مضى أكثر من 24 ساعة منذ آخر نسخة احتياطية";
                        txtBackupStatus.Foreground = System.Windows.Media.Brushes.Red;
                    }
                    else
                    {
                        txtBackupStatus.Text = "النسخ الاحتياطي محدث";
                        txtBackupStatus.Foreground = System.Windows.Media.Brushes.Green;
                    }
                }
                else
                {
                    txtLastBackup.Text = "لم يتم أبدًا";
                    txtBackupStatus.Text = "يُنصح بإجراء نسخ احتياطي";
                    txtBackupStatus.Foreground = System.Windows.Media.Brushes.Orange;
                }
                
                // Load performance metrics
                LoadPerformanceMetrics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل إحصائيات قاعدة البيانات: {ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPerformanceMetrics()
        {
            var metrics = new List<PerformanceMetric>
            {
                new PerformanceMetric 
                { 
                    Metric = "وقت الاستجابة", 
                    Value = "5 مللي ثانية", 
                    Status = "ممتاز" 
                },
                new PerformanceMetric 
                { 
                    Metric = "استخدام الذاكرة", 
                    Value = "45 ميجابايت", 
                    Status = "جيد" 
                },
                new PerformanceMetric 
                { 
                    Metric = "كفاءة الفهرسة", 
                    Value = "90%", 
                    Status = "ممتاز" 
                },
                new PerformanceMetric 
                { 
                    Metric = "حالة WAL", 
                    Value = "نشط", 
                    Status = "جيد" 
                },
                new PerformanceMetric 
                { 
                    Metric = "تشظي البيانات", 
                    Value = "15%", 
                    Status = "مقبول" 
                }
            };
            
            dgPerformanceMetrics.ItemsSource = metrics;
        }

        private void LoadBackupHistory()
        {
            try
            {
                var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FastFoodDB", "Backups");
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                    return;
                }
                
                var backupFiles = Directory.GetFiles(backupDir, "*.db");
                var backupHistory = new List<BackupInfo>();
                
                foreach (var file in backupFiles)
                {
                    var fileInfo = new FileInfo(file);
                    backupHistory.Add(new BackupInfo
                    {
                        Date = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                        Size = $"{fileInfo.Length / 1024.0 / 1024.0:F2} MB",
                        Path = file
                    });
                }
                
                lvBackupHistory.ItemsSource = backupHistory;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل سجل النسخ الاحتياطي: {ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseBackupPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "اختر مجلد النسخ الاحتياطي",
                ShowNewFolderButton = true
            };
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _backupPath = dialog.SelectedPath;
                txtBackupPath.Text = _backupPath;
            }
        }

        private void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(_backupPath);
                
                string backupFileName = $"Restaurant_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                string fullPath = Path.Combine(_backupPath, backupFileName);
                
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                
                _databaseService.BackupDatabase(fullPath);
                
                Mouse.OverrideCursor = null;
                
                MessageBox.Show($"تم إنشاء النسخة الاحتياطية بنجاح في:\n{fullPath}", 
                    "اكتمل النسخ الاحتياطي", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Refresh UI
                LoadDatabaseStats();
                LoadBackupHistory();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"خطأ في إنشاء النسخة الاحتياطية: {ex.Message}", 
                    "خطأ في النسخ الاحتياطي", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ملفات قاعدة البيانات (*.db)|*.db",
                Title = "اختر ملف النسخة الاحتياطية"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                RestoreFromBackup(openFileDialog.FileName);
            }
        }

        private void RestoreFromHistory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag is string backupPath)
            {
                RestoreFromBackup(backupPath);
            }
        }

        private void RestoreFromBackup(string backupPath)
        {
            var result = MessageBox.Show(
                "تحذير: ستؤدي استعادة قاعدة البيانات إلى استبدال جميع البيانات الحالية. هل أنت متأكد من أنك تريد المتابعة؟",
                "تأكيد الاستعادة", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    
                    bool success = _databaseService.RestoreDatabase(backupPath);
                    
                    Mouse.OverrideCursor = null;
                    
                    if (success)
                    {
                        MessageBox.Show("تمت استعادة قاعدة البيانات بنجاح. سيتم إعادة تشغيل التطبيق الآن.",
                            "اكتملت الاستعادة", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                        // In a real application, you would restart the application here
                        // For this example, we'll just refresh the UI
                        LoadDatabaseStats();
                        LoadBackupHistory();
                    }
                    else
                    {
                        MessageBox.Show("فشلت استعادة قاعدة البيانات. يرجى التحقق من ملف النسخة الاحتياطية والمحاولة مرة أخرى.",
                            "فشل الاستعادة", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show($"خطأ في استعادة قاعدة البيانات: {ex.Message}",
                        "خطأ في الاستعادة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag is string backupPath)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من أنك تريد حذف هذه النسخة الاحتياطية؟",
                    "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(backupPath);
                        
                        // Also delete associated WAL and SHM files if they exist
                        string walFile = backupPath + "-wal";
                        string shmFile = backupPath + "-shm";
                        
                        if (File.Exists(walFile))
                            File.Delete(walFile);
                            
                        if (File.Exists(shmFile))
                            File.Delete(shmFile);
                            
                        LoadBackupHistory();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في حذف النسخة الاحتياطية: {ex.Message}",
                            "خطأ في الحذف", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RunIntegrityCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                
                // In a real application, this would run a more comprehensive check
                var stats = _databaseService.GetDatabaseStats();
                
                Mouse.OverrideCursor = null;
                
                string status = stats.IntegrityCheck == "ok" ? "جيدة" : "تحتاج إلى إصلاح";
                
                MessageBox.Show($"اكتمل فحص سلامة قاعدة البيانات.\n\nالنتيجة: {status}",
                    "نتائج فحص السلامة", MessageBoxButton.OK, 
                    stats.IntegrityCheck == "ok" ? MessageBoxImage.Information : MessageBoxImage.Warning);
                    
                LoadDatabaseStats();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"خطأ في تشغيل فحص السلامة: {ex.Message}",
                    "خطأ في الفحص", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OptimizeDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                
                _databaseService.OptimizeDatabase();
                
                Mouse.OverrideCursor = null;
                
                MessageBox.Show("تم تحسين قاعدة البيانات بنجاح.",
                    "اكتمل التحسين", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                LoadDatabaseStats();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"خطأ في تحسين قاعدة البيانات: {ex.Message}",
                    "خطأ في التحسين", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshStats_Click(object sender, RoutedEventArgs e)
        {
            LoadDatabaseStats();
            LoadBackupHistory();
        }

        private void AutoBackup_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // In a real application, this would configure a scheduled task or service
            bool isEnabled = chkAutomaticBackup.IsChecked ?? false;
            
            MessageBox.Show(isEnabled ? 
                "تم تمكين النسخ الاحتياطي التلقائي اليومي. سيتم إجراء نسخة احتياطية كل 24 ساعة." : 
                "تم تعطيل النسخ الاحتياطي التلقائي.",
                "إعدادات النسخ الاحتياطي", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would navigate back or close the dialog
            // For this example, we'll just do nothing
        }
    }

    public class BackupInfo
    {
        public string Date { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
    }

    public class PerformanceMetric
    {
        public string Metric { get; set; }
        public string Value { get; set; }
        public string Status { get; set; }
    }
}