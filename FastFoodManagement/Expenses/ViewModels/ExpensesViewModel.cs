using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FastFoodManagement.Common.Commands;
using FastFoodManagement.Expenses.Models;
using FastFoodManagement.Services;
using Microsoft.Win32;

namespace FastFoodManagement.Expenses.ViewModels
{
    public class ExpensesViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        
        private ObservableCollection<Expense> _expenses;
        private ObservableCollection<string> _categories;
        private Expense _currentExpense;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedCategory;
        private decimal _totalExpenses;
        private string _receiptImagePath;
        private bool _isEditing;

        public ExpensesViewModel()
        {
            _dataService = new DataService();
            
            // Initialize collections
            _expenses = new ObservableCollection<Expense>();
            _categories = new ObservableCollection<string>
            {
                "Ingredients",
                "Utilities",
                "Rent",
                "Salaries",
                "Equipment",
                "Maintenance",
                "Marketing",
                "Office Supplies",
                "Taxes",
                "Other"
            };
            
            // Initialize commands
            SaveExpenseCommand = new RelayCommand(SaveExpense, CanSaveExpense);
            NewExpenseCommand = new RelayCommand(NewExpense);
            DeleteExpenseCommand = new RelayCommand<Expense>(DeleteExpense);
            EditExpenseCommand = new RelayCommand<Expense>(EditExpense);
            BrowseImageCommand = new RelayCommand(BrowseImage);
            FilterExpensesCommand = new RelayCommand(FilterExpenses);
            
            // Set default values
            StartDate = DateTime.Today.AddMonths(-1);
            EndDate = DateTime.Today;
            CurrentExpense = new Expense { Date = DateTime.Today };
            
            // Load initial data
            LoadExpenses();
        }

        #region Properties

        public ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public Expense CurrentExpense
        {
            get => _currentExpense;
            set
            {
                _currentExpense = value;
                OnPropertyChanged();
                (SaveExpenseCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set
            {
                _totalExpenses = value;
                OnPropertyChanged();
            }
        }

        public string ReceiptImagePath
        {
            get => _receiptImagePath;
            set
            {
                _receiptImagePath = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(_receiptImagePath))
                {
                    CurrentExpense.ReceiptImage = _receiptImagePath;
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SaveExpenseCommand { get; }
        public ICommand NewExpenseCommand { get; }
        public ICommand DeleteExpenseCommand { get; }
        public ICommand EditExpenseCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand FilterExpensesCommand { get; }

        #endregion

        #region Methods

        private async void LoadExpenses()
        {
            // In a real application, this would call a method in DataService
            // For now, we'll just create some sample data
            Expenses.Clear();
            
            // Sample data
            Expenses.Add(new Expense
            {
                Id = 1,
                Category = "Ingredients",
                Amount = 250.50m,
                Date = DateTime.Today.AddDays(-5),
                Description = "Weekly grocery purchase",
                Vendor = "Metro Supermarket"
            });
            
            Expenses.Add(new Expense
            {
                Id = 2,
                Category = "Utilities",
                Amount = 180.75m,
                Date = DateTime.Today.AddDays(-10),
                Description = "Electricity bill",
                Vendor = "Power Company"
            });
            
            Expenses.Add(new Expense
            {
                Id = 3,
                Category = "Rent",
                Amount = 1200.00m,
                Date = DateTime.Today.AddDays(-15),
                Description = "Monthly rent",
                Vendor = "Building Management"
            });
            
            CalculateTotalExpenses();
        }

        private void CalculateTotalExpenses()
        {
            TotalExpenses = Expenses.Sum(e => e.Amount);
        }

        private bool CanSaveExpense()
        {
            return CurrentExpense != null &&
                   !string.IsNullOrWhiteSpace(CurrentExpense.Category) &&
                   CurrentExpense.Amount > 0;
        }

        private async void SaveExpense()
        {
            try
            {
                if (IsEditing)
                {
                    // Update existing expense
                    // In a real application, this would call UpdateExpenseAsync in DataService
                    var existingExpense = Expenses.FirstOrDefault(e => e.Id == CurrentExpense.Id);
                    if (existingExpense != null)
                    {
                        int index = Expenses.IndexOf(existingExpense);
                        Expenses[index] = CurrentExpense;
                    }
                }
                else
                {
                    // Add new expense
                    // In a real application, this would call AddExpenseAsync in DataService
                    CurrentExpense.Id = Expenses.Count > 0 ? Expenses.Max(e => e.Id) + 1 : 1;
                    CurrentExpense.CreatedDate = DateTime.Now;
                    CurrentExpense.CreatedBy = "Current User"; // Would come from authentication
                    
                    Expenses.Add(CurrentExpense);
                }
                
                CalculateTotalExpenses();
                NewExpense();
                
                MessageBox.Show("Expense saved successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving expense: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewExpense()
        {
            CurrentExpense = new Expense { Date = DateTime.Today };
            ReceiptImagePath = null;
            IsEditing = false;
        }

        private void DeleteExpense(Expense expense)
        {
            if (expense == null) return;
            
            var result = MessageBox.Show("Are you sure you want to delete this expense?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // In a real application, this would call DeleteExpenseAsync in DataService
                Expenses.Remove(expense);
                CalculateTotalExpenses();
            }
        }

        private void EditExpense(Expense expense)
        {
            if (expense == null) return;
            
            // Create a copy to avoid modifying the original until save
            CurrentExpense = new Expense
            {
                Id = expense.Id,
                Category = expense.Category,
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                ReceiptImage = expense.ReceiptImage,
                PaymentMethod = expense.PaymentMethod,
                ReferenceNumber = expense.ReferenceNumber,
                IsRecurring = expense.IsRecurring,
                RecurringFrequency = expense.RecurringFrequency,
                Vendor = expense.Vendor,
                CreatedBy = expense.CreatedBy,
                CreatedDate = expense.CreatedDate,
                ModifiedBy = "Current User", // Would come from authentication
                ModifiedDate = DateTime.Now
            };
            
            ReceiptImagePath = expense.ReceiptImage;
            IsEditing = true;
        }

        private void BrowseImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png",
                Title = "Select Receipt Image"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                // In a real application, you might want to copy the file to a specific location
                // and store the relative path instead of the full path
                ReceiptImagePath = openFileDialog.FileName;
            }
        }

        private void FilterExpenses()
        {
            // In a real application, this would call a method in DataService to filter expenses
            // For this example, we'll just filter the existing collection
            
            LoadExpenses(); // Reload all expenses
            
            // Apply date filter
            var filtered = Expenses.Where(e => e.Date >= StartDate && e.Date <= EndDate);
            
            // Apply category filter if selected
            if (!string.IsNullOrEmpty(SelectedCategory))
            {
                filtered = filtered.Where(e => e.Category == SelectedCategory);
            }
            
            Expenses = new ObservableCollection<Expense>(filtered);
            CalculateTotalExpenses();
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