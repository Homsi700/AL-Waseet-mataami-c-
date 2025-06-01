using System;
using System.Threading.Tasks;
using System.Windows;

namespace FastFoodManagement.POS.Services
{
    public class POSService
    {
        // Payment methods enum
        public enum PaymentMethod
        {
            Cash,
            CreditCard,
            DebitCard,
            MobilePayment
        }

        // Process payment based on the selected method
        public bool ProcessPayment(decimal amount, string paymentMethodStr)
        {
            try
            {
                if (Enum.TryParse(paymentMethodStr, out PaymentMethod paymentMethod))
                {
                    switch (paymentMethod)
                    {
                        case PaymentMethod.Cash:
                            return ProcessCashPayment(amount);
                        case PaymentMethod.CreditCard:
                        case PaymentMethod.DebitCard:
                            return ProcessCardPayment(amount, paymentMethod);
                        case PaymentMethod.MobilePayment:
                            return ProcessMobilePayment(amount);
                        default:
                            MessageBox.Show("Unsupported payment method", "Payment Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                    }
                }
                else
                {
                    // Default to cash if method not recognized
                    return ProcessCashPayment(amount);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payment processing error: {ex.Message}", "Payment Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Process cash payment
        private bool ProcessCashPayment(decimal amount)
        {
            // In a real application, this might open a cash drawer
            // or prompt for amount tendered and calculate change
            return true;
        }

        // Process card payment (credit or debit)
        private bool ProcessCardPayment(decimal amount, PaymentMethod cardType)
        {
            // In a real application, this would integrate with a payment terminal
            // or payment gateway API
            
            // Simulate card processing
            return SimulateCardProcessing(amount);
        }

        // Process mobile payment
        private bool ProcessMobilePayment(decimal amount)
        {
            // In a real application, this would integrate with mobile payment APIs
            // like Apple Pay, Google Pay, etc.
            return true;
        }

        // Simulate card processing with a delay
        private bool SimulateCardProcessing(decimal amount)
        {
            // In a real application, this would be an async call to a payment processor
            Task.Delay(1500).Wait(); // Simulate processing time
            
            // Simulate successful transaction (in real app, would return result from payment processor)
            return true;
        }

        // Calculate change for cash payments
        public decimal CalculateChange(decimal amountTendered, decimal totalDue)
        {
            if (amountTendered < totalDue)
            {
                throw new ArgumentException("Amount tendered cannot be less than total due");
            }
            
            return Math.Round(amountTendered - totalDue, 2);
        }

        // Validate a credit card number using Luhn algorithm
        public bool ValidateCreditCardNumber(string cardNumber)
        {
            // Remove any non-digit characters
            cardNumber = new string(cardNumber.Where(char.IsDigit).ToArray());
            
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 13 || cardNumber.Length > 19)
            {
                return false;
            }

            // Luhn algorithm implementation
            int sum = 0;
            bool alternate = false;
            
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(cardNumber[i].ToString());
                
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }
                
                sum += digit;
                alternate = !alternate;
            }
            
            return sum % 10 == 0;
        }
    }
}