using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FastFoodManagement.Settings.Services
{
    public class SettingsService
    {
        // Save printer settings
        public void SavePrinterSettings(string printerName, string paperSize, bool defaultPrinter)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                
                config.AppSettings.Settings["DefaultPrinter"].Value = printerName;
                config.AppSettings.Settings["PaperSize"].Value = paperSize;
                config.AppSettings.Settings["UseDefaultPrinter"].Value = defaultPrinter.ToString();
                
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving printer settings: {ex.Message}", "Settings Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Load application settings
        public AppSettings LoadSettings()
        {
            try
            {
                return new AppSettings
                {
                    CompanyName = ConfigurationManager.AppSettings["CompanyName"],
                    CompanyAddress = ConfigurationManager.AppSettings["CompanyAddress"],
                    CompanyPhone = ConfigurationManager.AppSettings["CompanyPhone"],
                    CompanyEmail = ConfigurationManager.AppSettings["CompanyEmail"],
                    ReceiptFooter = ConfigurationManager.AppSettings["ReceiptFooter"],
                    DefaultPrinter = ConfigurationManager.AppSettings["DefaultPrinter"],
                    TaxRate = decimal.Parse(ConfigurationManager.AppSettings["TaxRate"] ?? "0.05"),
                    CurrencySymbol = ConfigurationManager.AppSettings["CurrencySymbol"],
                    BackupPath = ConfigurationManager.AppSettings["BackupPath"],
                    LogPath = ConfigurationManager.AppSettings["LogPath"]
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Settings Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Return default settings
                return new AppSettings
                {
                    CompanyName = "Fast Food Restaurant",
                    CompanyAddress = "123 Main Street",
                    CompanyPhone = "(555) 123-4567",
                    TaxRate = 0.05m,
                    CurrencySymbol = "$"
                };
            }
        }
        
        // Save general settings
        public void SaveGeneralSettings(AppSettings settings)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                
                config.AppSettings.Settings["CompanyName"].Value = settings.CompanyName;
                config.AppSettings.Settings["CompanyAddress"].Value = settings.CompanyAddress;
                config.AppSettings.Settings["CompanyPhone"].Value = settings.CompanyPhone;
                config.AppSettings.Settings["CompanyEmail"].Value = settings.CompanyEmail;
                config.AppSettings.Settings["ReceiptFooter"].Value = settings.ReceiptFooter;
                config.AppSettings.Settings["TaxRate"].Value = settings.TaxRate.ToString();
                config.AppSettings.Settings["CurrencySymbol"].Value = settings.CurrencySymbol;
                
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving general settings: {ex.Message}", "Settings Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Backup database
        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(backupPath);
                
                string backupFileName = $"FastFoodDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string fullPath = Path.Combine(backupPath, backupFileName);
                
                // Get connection string
                string connectionString = ConfigurationManager.ConnectionStrings["FastFoodConnection"].ConnectionString;
                
                // Create backup command
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    string backupCommand = $"BACKUP DATABASE FastFoodDB TO DISK = '{fullPath}'";
                    
                    using (SqlCommand command = new SqlCommand(backupCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                
                MessageBox.Show($"Database backup created successfully at:\n{fullPath}", 
                    "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error backing up database: {ex.Message}", "Backup Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Restore database
        public async Task<bool> RestoreDatabaseAsync(string backupFile)
        {
            try
            {
                if (!File.Exists(backupFile))
                {
                    MessageBox.Show("Backup file does not exist.", "Restore Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                
                // Get connection string
                string connectionString = ConfigurationManager.ConnectionStrings["FastFoodConnection"].ConnectionString;
                
                // Create restore command
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // First, set database to single user mode
                    string singleUserCommand = "ALTER DATABASE FastFoodDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    
                    using (SqlCommand command = new SqlCommand(singleUserCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    
                    // Restore database
                    string restoreCommand = $"RESTORE DATABASE FastFoodDB FROM DISK = '{backupFile}' WITH REPLACE";
                    
                    using (SqlCommand command = new SqlCommand(restoreCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    
                    // Set database back to multi-user mode
                    string multiUserCommand = "ALTER DATABASE FastFoodDB SET MULTI_USER";
                    
                    using (SqlCommand command = new SqlCommand(multiUserCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                
                MessageBox.Show("Database restored successfully.", "Restore Complete", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring database: {ex.Message}", "Restore Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Get available printers
        public string[] GetAvailablePrinters()
        {
            return System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
        }
    }
    
    public class AppSettings
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string ReceiptFooter { get; set; }
        public string DefaultPrinter { get; set; }
        public decimal TaxRate { get; set; }
        public string CurrencySymbol { get; set; }
        public string BackupPath { get; set; }
        public string LogPath { get; set; }
    }
}