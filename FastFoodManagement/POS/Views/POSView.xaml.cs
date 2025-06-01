using System.Windows;
using System.Windows.Controls;
using FastFoodManagement.POS.ViewModels;

namespace FastFoodManagement.POS.Views
{
    /// <summary>
    /// Interaction logic for POSView.xaml
    /// </summary>
    public partial class POSView : Page
    {
        private POSViewModel _viewModel;

        public POSView()
        {
            InitializeComponent();
            _viewModel = Resources["ViewModel"] as POSViewModel;
        }

        private void PaymentMethodSelected(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && _viewModel != null)
            {
                string paymentMethod = button.Tag.ToString();
                _viewModel.ProcessPayment(paymentMethod);
            }
        }
    }
}