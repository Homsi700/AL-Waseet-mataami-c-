using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FastFoodManagement.Common.Commands;
using FastFoodManagement.Settings.Services;
using Microsoft.Win32;

namespace FastFoodManagement.Settings.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        
        private AppSettings _settings;
        private ObservableCollection<string> _availablePrinters;
        private string _selectedPrinter;
        private string _selectedPaperSize;
        private bool _useDefaultPrinter;
        private string _backupPath;
        private bool _isBusy;

        public SettingsViewModel()
        {
            _settingsService = new SettingsService();
            
            // Initialize commands
            SaveGeneralSettingsCommand = new RelayCommand(SaveGeneralSettings);
            SavePrinterSettingsCommand = new RelayCommand(SavePrinterSettings);
            BackupDatabaseCommand = new RelayCommand(BackupDatabase);
            RestoreDatabaseCommand = new RelayCommand(RestoreDatabase);
            BrowseBackupPathCommand = new RelayCommand(BrowseBackupPath);
            
            // Load settings
            LoadSettings();
            LoadPrinters();
        }

        #region Properties

        public AppSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> AvailablePrinters
        {
            get => _availablePrinters;
            set
            {
                _availablePrinters = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set
            {
                _selectedPrinter = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPaperSize
        {
            get => _selectedPaperSize;
            set
            {
                _selectedPaperSize = value;
                OnPropertyChanged();
            }
        }

        public bool UseDefaultPrinter
        {
            get => _useDefaultPrinter;
            set
            {
                _useDefaultPrinter = value;
                OnPropertyChanged();
            }
        }

        public string BackupPath
        {
            get => _backupPath;
            set
            {
                _backupPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SaveGeneralSettingsCommand { get; }
        public ICommand SavePrinterSettingsCommand { get; }
        public ICommand BackupDatabaseCommand { get; }
        public ICommand RestoreDatabaseCommand { get; }
        public ICommand BrowseBackupPathCommand { get; }

        #endregion

        #region Methods

        private void LoadSettings()
        {
            Settings = _settingsService.LoadSettings();
            BackupPath = Settings.BackupPath;
        }

        private void LoadPrinters()
        {
            var printers = _settingsService.GetAvailablePrinters();
            AvailablePrinters = new ObservableCollection<string>(printers);
            
            SelectedPrinter = Settings.DefaultPrinter;
            SelectedPaperSize = "Letter"; // Default
            UseDefaultPrinter = true; // Default
        }

        private void SaveGeneralSettings()
        {
            try
            {
                _settingsService.SaveGeneralSettings(Settings);
                MessageBox.Show("General settings saved successfully.", "Settings Saved", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving general settings: {ex.Message}", "Settings Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePrinterSettings()
        {
            try
            {
                _settingsService.SavePrinterSettings(SelectedPrinter, SelectedPaperSize, UseDefaultPrinter);
                MessageBox.Show("Printer settings saved successfully.", "Settings Saved", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving printer settings: {ex.Message}", "Settings Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BackupDatabase()
        {
            if (string.IsNullOrWhiteSpace(BackupPath))
            {
                MessageBox.Show("Please select a backup path first.", "Backup Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            IsBusy = true;
            
            try
            {
                await _settingsService.BackupDatabaseAsync(BackupPath);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void RestoreDatabase()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Backup files (*.bak)|*.bak",
                Title = "Select Database Backup"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                IsBusy = true;
                
                try
                {
                    await _settingsService.RestoreDatabaseAsync(openFileDialog.FileName);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private void BrowseBackupPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Backup Directory",
                ShowNewFolderButton = true
            };
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BackupPath = dialog.SelectedPath;
            }
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

    // RelayCommand is now defined in FastFoodManagement.Common.Commands namespace
}