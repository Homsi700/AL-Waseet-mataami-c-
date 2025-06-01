using System;
using System.Windows;
using System.Windows.Threading;

namespace FastFoodManagement.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
            
            // Set default page
            MainFrame.Navigate(new Uri("Views/POSView.xaml", UriKind.Relative));
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Initialize with current time
            UpdateDateTime();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            txtDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void btnPOS_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/POSView.xaml", UriKind.Relative));
        }

        private void btnProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/ProductsView.xaml", UriKind.Relative));
        }

        private void btnCategories_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/CategoriesView.xaml", UriKind.Relative));
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/OrdersView.xaml", UriKind.Relative));
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/ReportsView.xaml", UriKind.Relative));
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/SettingsView.xaml", UriKind.Relative));
        }
    }
}