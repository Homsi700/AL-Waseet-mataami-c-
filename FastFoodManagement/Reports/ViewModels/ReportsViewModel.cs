using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FastFoodManagement.Common.Commands;
using FastFoodManagement.Reports.Services;
using FastFoodManagement.Services;

namespace FastFoodManagement.Reports.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private readonly ReportGenerator _reportGenerator;
        private readonly PrintService _printService;

        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedReportType;
        private bool _isGenerating;
        private string _reportTitle;
        private decimal _totalSales;
        private int _orderCount;
        private decimal _averageOrderValue;
        private string _topSellingItem;

        public ReportsViewModel()
        {
            _dataService = new DataService();
            _reportGenerator = new ReportGenerator();
            _printService = new PrintService();

            // Initialize collections
            DailySales = new ObservableCollection<SalesData>();
            TopProducts = new ObservableCollection<TopProduct>();
            SalesByCategory = new ObservableCollection<CategorySales>();

            // Initialize commands
            GenerateReportCommand = new RelayCommand(GenerateReport, CanGenerateReport);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, () => !IsGenerating && DailySales.Count > 0);
            PrintReportCommand = new RelayCommand(PrintReport, () => !IsGenerating && DailySales.Count > 0);

            // Set default dates
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
            SelectedReportType = "Sales by Product";
        }

        #region Properties

        public ObservableCollection<SalesData> DailySales { get; set; }
        public ObservableCollection<TopProduct> TopProducts { get; set; }
        public ObservableCollection<CategorySales> SalesByCategory { get; set; }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
                (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
                (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                _selectedReportType = value;
                OnPropertyChanged();
            }
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                _isGenerating = value;
                OnPropertyChanged();
                (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExportToExcelCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (PrintReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ReportTitle
        {
            get => _reportTitle;
            set
            {
                _reportTitle = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalSales
        {
            get => _totalSales;
            set
            {
                _totalSales = value;
                OnPropertyChanged();
            }
        }

        public int OrderCount
        {
            get => _orderCount;
            set
            {
                _orderCount = value;
                OnPropertyChanged();
            }
        }

        public decimal AverageOrderValue
        {
            get => _averageOrderValue;
            set
            {
                _averageOrderValue = value;
                OnPropertyChanged();
            }
        }

        public string TopSellingItem
        {
            get => _topSellingItem;
            set
            {
                _topSellingItem = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand PrintReportCommand { get; }

        #endregion

        #region Methods

        private bool CanGenerateReport()
        {
            return !IsGenerating && StartDate <= EndDate;
        }

        private async void GenerateReport()
        {
            IsGenerating = true;
            ReportTitle = $"{SelectedReportType}: {StartDate:d} - {EndDate:d}";

            try
            {
                switch (SelectedReportType)
                {
                    case "Sales by Product":
                        await LoadSalesReport();
                        break;
                    case "Sales by Category":
                        await LoadCategorySalesReport();
                        break;
                    case "Daily Sales":
                        await LoadDailySalesReport();
                        break;
                    case "Monthly Sales":
                        await LoadMonthlySalesReport();
                        break;
                }

                // Load summary data
                await LoadSummaryData();
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private async Task LoadSalesReport()
        {
            var salesByProduct = await _dataService.GetSalesByProductAsync(StartDate, EndDate);
            
            TopProducts.Clear();
            foreach (var item in salesByProduct)
            {
                TopProducts.Add(new TopProduct
                {
                    ProductName = item.Key,
                    TotalSales = item.Value
                });
            }
        }

        private async Task LoadCategorySalesReport()
        {
            var salesByCategory = await _dataService.GetSalesByCategoryAsync(StartDate, EndDate);
            
            SalesByCategory.Clear();
            foreach (var item in salesByCategory)
            {
                SalesByCategory.Add(new CategorySales
                {
                    CategoryName = item.Key,
                    TotalSales = item.Value
                });
            }
        }

        private async Task LoadDailySalesReport()
        {
            var orders = await _dataService.GetOrdersByDateRangeAsync(StartDate, EndDate);
            
            // Group orders by date
            var dailySales = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesData
                {
                    Date = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                });
            
            DailySales.Clear();
            foreach (var day in dailySales)
            {
                DailySales.Add(day);
            }
        }

        private async Task LoadMonthlySalesReport()
        {
            var orders = await _dataService.GetOrdersByDateRangeAsync(StartDate, EndDate);
            
            // Group orders by month
            var monthlySales = orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new SalesData
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                });
            
            DailySales.Clear();
            foreach (var month in monthlySales)
            {
                DailySales.Add(month);
            }
        }

        private async Task LoadSummaryData()
        {
            TotalSales = await _dataService.GetTotalSalesAsync(StartDate, EndDate);
            OrderCount = await _dataService.GetOrderCountAsync(StartDate, EndDate);
            
            if (OrderCount > 0)
            {
                AverageOrderValue = Math.Round(TotalSales / OrderCount, 2);
            }
            else
            {
                AverageOrderValue = 0;
            }

            // Find top selling item
            if (TopProducts.Count > 0)
            {
                var topProduct = TopProducts
                    .OrderByDescending(p => p.TotalSales)
                    .FirstOrDefault();
                
                TopSellingItem = topProduct?.ProductName ?? "None";
            }
            else
            {
                TopSellingItem = "None";
            }
        }

        private void ExportToExcel()
        {
            switch (SelectedReportType)
            {
                case "Sales by Product":
                    _reportGenerator.ExportToExcel(TopProducts, ReportTitle);
                    break;
                case "Sales by Category":
                    _reportGenerator.ExportToExcel(SalesByCategory, ReportTitle);
                    break;
                case "Daily Sales":
                case "Monthly Sales":
                    _reportGenerator.ExportToExcel(DailySales, ReportTitle);
                    break;
            }
        }

        private void PrintReport()
        {
            var reportDocument = _reportGenerator.GenerateReportDocument(
                SelectedReportType, 
                StartDate, 
                EndDate, 
                DailySales, 
                TopProducts, 
                SalesByCategory,
                TotalSales,
                OrderCount,
                AverageOrderValue,
                TopSellingItem);
            
            _printService.PrintReport(reportDocument, ReportTitle);
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

    public class SalesData
    {
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProduct
    {
        public string ProductName { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class CategorySales
    {
        public string CategoryName { get; set; }
        public decimal TotalSales { get; set; }
    }

    // RelayCommand is now defined in FastFoodManagement.Common.Commands namespace
}