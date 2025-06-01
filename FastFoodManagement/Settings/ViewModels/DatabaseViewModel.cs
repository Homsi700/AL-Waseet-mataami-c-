using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FastFoodManagement.Common.Commands;
using FastFoodManagement.Data;

namespace FastFoodManagement.Settings.ViewModels
{
    public class DatabaseViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private string _backupPath;
        private bool _isAutomaticBackupEnabled;
        private bool _isBusy;
        private DatabaseStats _databaseStats;
        private ObservableCollection<BackupInfo> _backupHistory;
        private ObservableCollection<PerformanceMetric> _performanceMetrics;

        public DatabaseViewModel()
        {
            _databaseService = new DatabaseService();
            
            // Initialize commands
            CreateBackupCommand = new RelayCommand(CreateBackup, () => !IsBusy);
            RestoreBackupCommand = new RelayCommand<string>(RestoreBackup, path => !IsBusy);
            DeleteBackupCommand = new RelayCommand<string>(DeleteBackup, path => !IsBusy);
            OptimizeDatabaseCommand = new RelayCommand(OptimizeDatabase, () => !IsBusy);
            RunIntegrityCheckCommand = new RelayCommand(RunIntegrityCheck, () => !IsBusy);
            RefreshStatsCommand = new RelayCommand(RefreshStats, () => !IsBusy);
            BrowseBackupPathCommand = new RelayCommand(BrowseBackupPath);
            
            // Set default backup path
            BackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FastFoodDB", "Backups");
            
            // Load initial data
            LoadDatabaseStats();
            LoadBackupHistory();
        }

        #region Properties

        public string BackupPath
        {
            get => _backupPath;
            set
            {
                _backupPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsAutomaticBackupEnabled
        {
            get => _isAutomaticBackupEnabled;
            set
            {
                _isAutomaticBackupEnabled = value;
                OnPropertyChanged();
                
                // In a real application, this would update a setting in the database or config file
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                
                // Update command can execute status
                (CreateBackupCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (OptimizeDatabaseCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RunIntegrityCheckCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RefreshStatsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public DatabaseStats DatabaseStats
        {
            get => _databaseStats;
            set
            {
                _databaseStats = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BackupInfo> BackupHistory
        {
            get => _backupHistory;
            set
            {
                _backupHistory = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PerformanceMetric> PerformanceMetrics
        {
            get => _performanceMetrics;
            set
            {
                _performanceMetrics = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand CreateBackupCommand { get; }
        public ICommand RestoreBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand OptimizeDatabaseCommand { get; }
        public ICommand RunIntegrityCheckCommand { get; }
        public ICommand RefreshStatsCommand { get; }
        public ICommand BrowseBackupPathCommand { get; }

        #endregion

        #region Methods

        private void LoadDatabaseStats()
        {
            try
            {
                DatabaseStats = _databaseService.GetDatabaseStats();
                
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
            PerformanceMetrics = new ObservableCollection<PerformanceMetric>
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
        }

        private void LoadBackupHistory()
        {
            try
            {
                var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FastFoodDB", "Backups");
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                    BackupHistory = new ObservableCollection<BackupInfo>();
                    return;
                }
                
                var backupFiles = Directory.GetFiles(backupDir, "*.db");
                var backupHistory = new ObservableCollection<BackupInfo>();
                
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
                
                BackupHistory = backupHistory;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل سجل النسخ الاحتياطي: {ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseBackupPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "اختر مجلد النسخ الاحتياطي",
                ShowNewFolderButton = true
            };
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BackupPath = dialog.SelectedPath;
            }
        }

        private async void CreateBackup()
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(BackupPath);
                
                string backupFileName = $"Restaurant_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                string fullPath = Path.Combine(BackupPath, backupFileName);
                
                IsBusy = true;
                
                await Task.Run(() => _databaseService.BackupDatabase(fullPath));
                
                IsBusy = false;
                
                MessageBox.Show($"تم إنشاء النسخة الاحتياطية بنجاح في:\n{fullPath}", 
                    "اكتمل النسخ الاحتياطي", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Refresh UI
                LoadDatabaseStats();
                LoadBackupHistory();
            }
            catch (Exception ex)
            {
                IsBusy = false;
                MessageBox.Show($"خطأ في إنشاء النسخة الاحتياطية: {ex.Message}", 
                    "خطأ في النسخ الاحتياطي", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RestoreBackup(string backupPath)
        {
            if (string.IsNullOrEmpty(backupPath))
            {
                // Open file dialog to select backup file
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "ملفات قاعدة البيانات (*.db)|*.db",
                    Title = "اختر ملف النسخة الاحتياطية"
                };
                
                if (openFileDialog.ShowDialog() == true)
                {
                    backupPath = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }
            
            var result = MessageBox.Show(
                "تحذير: ستؤدي استعادة قاعدة البيانات إلى استبدال جميع البيانات الحالية. هل أنت متأكد من أنك تريد المتابعة؟",
                "تأكيد الاستعادة", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    
                    bool success = await Task.Run(() => _databaseService.RestoreDatabase(backupPath));
                    
                    IsBusy = false;
                    
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
                    IsBusy = false;
                    MessageBox.Show($"خطأ في استعادة قاعدة البيانات: {ex.Message}",
                        "خطأ في الاستعادة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteBackup(string backupPath)
        {
            if (string.IsNullOrEmpty(backupPath))
                return;
                
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

        private async void RunIntegrityCheck()
        {
            try
            {
                IsBusy = true;
                
                // In a real application, this would run a more comprehensive check
                var stats = await Task.Run(() => _databaseService.GetDatabaseStats());
                
                IsBusy = false;
                
                string status = stats.IntegrityCheck == "ok" ? "جيدة" : "تحتاج إلى إصلاح";
                
                MessageBox.Show($"اكتمل فحص سلامة قاعدة البيانات.\n\nالنتيجة: {status}",
                    "نتائج فحص السلامة", MessageBoxButton.OK, 
                    stats.IntegrityCheck == "ok" ? MessageBoxImage.Information : MessageBoxImage.Warning);
                    
                LoadDatabaseStats();
            }
            catch (Exception ex)
            {
                IsBusy = false;
                MessageBox.Show($"خطأ في تشغيل فحص السلامة: {ex.Message}",
                    "خطأ في الفحص", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OptimizeDatabase()
        {
            try
            {
                IsBusy = true;
                
                await Task.Run(() => _databaseService.OptimizeDatabase());
                
                IsBusy = false;
                
                MessageBox.Show("تم تحسين قاعدة البيانات بنجاح.",
                    "اكتمل التحسين", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                LoadDatabaseStats();
            }
            catch (Exception ex)
            {
                IsBusy = false;
                MessageBox.Show($"خطأ في تحسين قاعدة البيانات: {ex.Message}",
                    "خطأ في التحسين", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshStats()
        {
            LoadDatabaseStats();
            LoadBackupHistory();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
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

    // RelayCommand is now defined in FastFoodManagement.Common.Commands namespace
}