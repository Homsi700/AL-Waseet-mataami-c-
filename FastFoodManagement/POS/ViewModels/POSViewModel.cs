using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FastFoodManagement.Common.Commands;
using FastFoodManagement.Data.Models;
using FastFoodManagement.POS.Services;
using FastFoodManagement.Services;

namespace FastFoodManagement.POS.ViewModels
{
    public class POSViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private readonly POSService _posService;
        private readonly PrintService _printService;

        private ObservableCollection<Product> _products;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<OrderItem> _currentOrder;
        private Category _selectedCategory;
        private decimal _subtotal;
        private decimal _tax;
        private decimal _total;
        private string _customerName;
        private string _searchText;

        public POSViewModel()
        {
            _dataService = new DataService();
            _posService = new POSService();
            _printService = new PrintService();

            // Initialize collections
            _products = new ObservableCollection<Product>();
            _categories = new ObservableCollection<Category>();
            _currentOrder = new ObservableCollection<OrderItem>();

            // Initialize commands
            AddToOrderCommand = new RelayCommand<Product>(AddToOrder);
            RemoveFromOrderCommand = new RelayCommand<OrderItem>(RemoveFromOrder);
            ClearOrderCommand = new RelayCommand(ClearOrder);
            CheckoutCommand = new RelayCommand(Checkout, CanCheckout);
            CategorySelectionCommand = new RelayCommand<Category>(SelectCategory);
            SearchCommand = new RelayCommand(PerformSearch);

            // Load initial data
            LoadCategories();
            LoadProducts();
        }

        #region Properties

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<OrderItem> CurrentOrder
        {
            get => _currentOrder;
            set
            {
                _currentOrder = value;
                OnPropertyChanged();
                CalculateTotal();
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                if (_selectedCategory != null)
                {
                    LoadProductsByCategory(_selectedCategory.CategoryId);
                }
            }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set
            {
                _subtotal = value;
                OnPropertyChanged();
            }
        }

        public decimal Tax
        {
            get => _tax;
            set
            {
                _tax = value;
                OnPropertyChanged();
            }
        }

        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged();
            }
        }

        public string CustomerName
        {
            get => _customerName;
            set
            {
                _customerName = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                if (string.IsNullOrWhiteSpace(_searchText))
                {
                    // If search is cleared, reload products based on selected category
                    if (_selectedCategory != null)
                    {
                        LoadProductsByCategory(_selectedCategory.CategoryId);
                    }
                    else
                    {
                        LoadProducts();
                    }
                }
            }
        }

        #endregion

        #region Commands

        public ICommand AddToOrderCommand { get; }
        public ICommand RemoveFromOrderCommand { get; }
        public ICommand ClearOrderCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand CategorySelectionCommand { get; }
        public ICommand SearchCommand { get; }

        #endregion

        #region Methods

        private async void LoadCategories()
        {
            var categories = await _dataService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private async void LoadProducts()
        {
            var products = await _dataService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in products.Where(p => p.IsAvailable))
            {
                Products.Add(product);
            }
        }

        private async void LoadProductsByCategory(int categoryId)
        {
            var products = await _dataService.GetProductsByCategoryAsync(categoryId);
            Products.Clear();
            foreach (var product in products.Where(p => p.IsAvailable))
            {
                Products.Add(product);
            }
        }

        private void AddToOrder(Product product)
        {
            if (product == null) return;

            // Check if the product is already in the order
            var existingItem = CurrentOrder.FirstOrDefault(item => item.ProductId == product.ProductId);

            if (existingItem != null)
            {
                // Increment quantity if already in order
                existingItem.Quantity++;
                existingItem.Subtotal = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                // Add new item to order
                var orderItem = new OrderItem
                {
                    ProductId = product.ProductId,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price,
                    Subtotal = product.Price
                };
                CurrentOrder.Add(orderItem);
            }

            CalculateTotal();
        }

        private void RemoveFromOrder(OrderItem item)
        {
            if (item == null) return;

            if (item.Quantity > 1)
            {
                // Decrement quantity
                item.Quantity--;
                item.Subtotal = item.Quantity * item.UnitPrice;
            }
            else
            {
                // Remove item completely
                CurrentOrder.Remove(item);
            }

            CalculateTotal();
        }

        private void ClearOrder()
        {
            CurrentOrder.Clear();
            CalculateTotal();
            CustomerName = string.Empty;
        }

        private void CalculateTotal()
        {
            Subtotal = CurrentOrder.Sum(item => item.Subtotal);
            
            // Calculate tax (assuming 5% tax rate, could be configurable)
            decimal taxRate = 0.05m;
            Tax = Math.Round(Subtotal * taxRate, 2);
            
            Total = Subtotal + Tax;
            
            // Refresh can checkout status
            (CheckoutCommand as RelayCommand).RaiseCanExecuteChanged();
        }

        private bool CanCheckout()
        {
            return CurrentOrder.Count > 0;
        }

        private async void Checkout()
        {
            // Create a new order
            var order = new Order
            {
                OrderDate = DateTime.Now,
                CustomerName = CustomerName,
                TotalAmount = Total,
                IsPaid = true,
                PaymentMethod = "Cash", // This could be selected by the user
                OrderStatus = "Completed"
            };

            // Add order items
            foreach (var item in CurrentOrder)
            {
                order.OrderItems.Add(item);
            }

            // Save order to database
            bool success = await _dataService.AddOrderAsync(order);

            if (success)
            {
                // Print receipt
                _printService.PrintReceipt(order);
                
                // Clear the current order
                ClearOrder();
            }
        }

        private void SelectCategory(Category category)
        {
            SelectedCategory = category;
        }

        private async void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            // Search products by name
            var allProducts = await _dataService.GetAllProductsAsync();
            var filteredProducts = allProducts
                .Where(p => p.IsAvailable && p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Products.Clear();
            foreach (var product in filteredProducts)
            {
                Products.Add(product);
            }
        }

        public void ProcessPayment(string paymentMethod)
        {
            // Process different payment methods (Cash, Card, etc.)
            _posService.ProcessPayment(Total, paymentMethod);
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