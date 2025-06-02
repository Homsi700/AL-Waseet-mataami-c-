using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.IO;
using System.IO.Packaging;
using System.Printing;
using FastFoodManagement.Data.Models;

namespace FastFoodManagement.Services
{
    public class PrintService
    {
        public void PrintReceipt(Order order)
        {
            try
            {
                // Create a PrintDialog
                PrintDialog printDialog = new PrintDialog();
                
                if (printDialog.ShowDialog() == true)
                {
                    // Create the visual elements for the receipt
                    FlowDocument document = CreateReceiptDocument(order);
                    
                    // Create a DocumentPaginator
                    IDocumentPaginatorSource dps = document;
                    
                    // Print the document
                    printDialog.PrintDocument(dps.DocumentPaginator, "Order Receipt");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing receipt: {ex.Message}", "Print Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateReceiptDocument(Order order)
        {
            // Create a FlowDocument
            FlowDocument document = new FlowDocument();
            document.PagePadding = new Thickness(50);
            document.ColumnWidth = 300;

            // Add header
            Paragraph header = new Paragraph(new Run("RECEIPT"));
            header.FontSize = 20;
            header.FontWeight = FontWeights.Bold;
            header.TextAlignment = TextAlignment.Center;
            document.Blocks.Add(header);

            // Add order details
            Paragraph orderDetails = new Paragraph();
            orderDetails.Inlines.Add(new Run($"Order #: {order.OrderId}\n"));
            orderDetails.Inlines.Add(new Run($"Date: {order.OrderDate}\n"));
            if (!string.IsNullOrEmpty(order.CustomerName))
                orderDetails.Inlines.Add(new Run($"Customer: {order.CustomerName}\n"));
            orderDetails.Inlines.Add(new Run("-----------------------------------\n"));
            document.Blocks.Add(orderDetails);

            // Add order items
            Table itemsTable = new Table();
            itemsTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
            itemsTable.Columns.Add(new TableColumn { Width = new GridLength(50) });
            itemsTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            
            // Add header row
            TableRowGroup headerRow = new TableRowGroup();
            TableRow hr = new TableRow();
            hr.Cells.Add(new TableCell(new Paragraph(new Run("Item"))));
            hr.Cells.Add(new TableCell(new Paragraph(new Run("Qty"))));
            hr.Cells.Add(new TableCell(new Paragraph(new Run("Price"))));
            headerRow.Rows.Add(hr);
            itemsTable.RowGroups.Add(headerRow);

            // Add item rows
            TableRowGroup itemRows = new TableRowGroup();
            foreach (var item in order.OrderItems)
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.Subtotal:C}"))));
                itemRows.Rows.Add(row);
            }
            itemsTable.RowGroups.Add(itemRows);
            document.Blocks.Add(itemsTable);

            // Add total
            Paragraph total = new Paragraph();
            total.Inlines.Add(new Run("-----------------------------------\n"));
            total.Inlines.Add(new Run($"Total: {order.TotalAmount:C}\n"));
            total.Inlines.Add(new Run($"Payment Method: {order.PaymentMethod}\n"));
            total.FontWeight = FontWeights.Bold;
            document.Blocks.Add(total);

            // Add footer
            Paragraph footer = new Paragraph(new Run("Thank you for your order!"));
            footer.TextAlignment = TextAlignment.Center;
            footer.FontStyle = FontStyles.Italic;
            document.Blocks.Add(footer);

            return document;
        }

        public void PrintReport(FlowDocument reportDocument, string reportTitle)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                
                if (printDialog.ShowDialog() == true)
                {
                    IDocumentPaginatorSource dps = reportDocument;
                    printDialog.PrintDocument(dps.DocumentPaginator, reportTitle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Print Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}