using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using FastFoodManagement.Reports.ViewModels;
using Microsoft.Win32;

namespace FastFoodManagement.Reports.Services
{
    public class ReportGenerator
    {
        public FlowDocument GenerateReportDocument(
            string reportType,
            DateTime startDate,
            DateTime endDate,
            IEnumerable<SalesData> dailySales,
            IEnumerable<TopProduct> topProducts,
            IEnumerable<CategorySales> categorySales,
            decimal totalSales,
            int orderCount,
            decimal averageOrderValue,
            string topSellingItem)
        {
            // Create a FlowDocument
            FlowDocument document = new FlowDocument();
            document.PagePadding = new Thickness(50);
            document.ColumnWidth = 700;

            // Add header
            Paragraph header = new Paragraph(new Run(reportType));
            header.FontSize = 24;
            header.FontWeight = FontWeights.Bold;
            header.TextAlignment = TextAlignment.Center;
            document.Blocks.Add(header);

            // Add date range
            Paragraph dateRange = new Paragraph(new Run($"Period: {startDate:d} - {endDate:d}"));
            dateRange.FontSize = 14;
            dateRange.TextAlignment = TextAlignment.Center;
            dateRange.Margin = new Thickness(0, 0, 0, 20);
            document.Blocks.Add(dateRange);

            // Add summary section
            Section summarySection = new Section();
            Paragraph summaryHeader = new Paragraph(new Run("Summary"));
            summaryHeader.FontSize = 18;
            summaryHeader.FontWeight = FontWeights.Bold;
            summaryHeader.Margin = new Thickness(0, 0, 0, 10);
            summarySection.Blocks.Add(summaryHeader);

            // Add summary table
            Table summaryTable = new Table();
            summaryTable.CellSpacing = 0;
            summaryTable.Margin = new Thickness(0, 0, 0, 20);

            // Define columns
            summaryTable.Columns.Add(new TableColumn { Width = new GridLength(200) });
            summaryTable.Columns.Add(new TableColumn { Width = new GridLength(200) });

            // Add header row
            TableRowGroup summaryRows = new TableRowGroup();
            summaryTable.RowGroups.Add(summaryRows);

            // Add data rows
            AddTableRow(summaryRows, "Total Sales:", $"{totalSales:C}");
            AddTableRow(summaryRows, "Number of Orders:", orderCount.ToString());
            AddTableRow(summaryRows, "Average Order Value:", $"{averageOrderValue:C}");
            AddTableRow(summaryRows, "Top Selling Item:", topSellingItem);

            summarySection.Blocks.Add(summaryTable);
            document.Blocks.Add(summarySection);

            // Add report-specific data
            switch (reportType)
            {
                case "Sales by Product":
                    AddProductSalesSection(document, topProducts);
                    break;
                case "Sales by Category":
                    AddCategorySalesSection(document, categorySales);
                    break;
                case "Daily Sales":
                    AddDailySalesSection(document, dailySales);
                    break;
                case "Monthly Sales":
                    AddMonthlySalesSection(document, dailySales);
                    break;
            }

            // Add footer
            Paragraph footer = new Paragraph(new Run($"Report generated on {DateTime.Now}"));
            footer.FontStyle = FontStyles.Italic;
            footer.TextAlignment = TextAlignment.Center;
            footer.Margin = new Thickness(0, 20, 0, 0);
            document.Blocks.Add(footer);

            return document;
        }

        private void AddTableRow(TableRowGroup rowGroup, string label, string value)
        {
            TableRow row = new TableRow();
            
            Paragraph labelPara = new Paragraph(new Run(label));
            labelPara.FontWeight = FontWeights.Bold;
            row.Cells.Add(new TableCell(labelPara));
            
            row.Cells.Add(new TableCell(new Paragraph(new Run(value))));
            
            rowGroup.Rows.Add(row);
        }

        private void AddProductSalesSection(FlowDocument document, IEnumerable<TopProduct> products)
        {
            Section section = new Section();
            Paragraph header = new Paragraph(new Run("Sales by Product"));
            header.FontSize = 18;
            header.FontWeight = FontWeights.Bold;
            header.Margin = new Thickness(0, 0, 0, 10);
            section.Blocks.Add(header);

            // Create table
            Table table = new Table();
            table.CellSpacing = 0;
            table.Borders = new Thickness(1);
            table.BorderBrush = Brushes.Gray;

            // Define columns
            table.Columns.Add(new TableColumn { Width = new GridLength(300) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });

            // Add header row
            TableRowGroup headerRow = new TableRowGroup();
            TableRow hr = new TableRow();
            hr.Background = Brushes.LightGray;
            
            Paragraph productHeader = new Paragraph(new Run("Product"));
            productHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(productHeader));
            
            Paragraph salesHeader = new Paragraph(new Run("Sales"));
            salesHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(salesHeader));
            
            headerRow.Rows.Add(hr);
            table.RowGroups.Add(headerRow);

            // Add data rows
            TableRowGroup dataRows = new TableRowGroup();
            foreach (var product in products.OrderByDescending(p => p.TotalSales))
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(product.ProductName))));
                
                Paragraph salesPara = new Paragraph(new Run($"{product.TotalSales:C}"));
                salesPara.TextAlignment = TextAlignment.Right;
                row.Cells.Add(new TableCell(salesPara));
                
                dataRows.Rows.Add(row);
            }
            table.RowGroups.Add(dataRows);

            section.Blocks.Add(table);
            document.Blocks.Add(section);
        }

        private void AddCategorySalesSection(FlowDocument document, IEnumerable<CategorySales> categories)
        {
            Section section = new Section();
            Paragraph header = new Paragraph(new Run("Sales by Category"));
            header.FontSize = 18;
            header.FontWeight = FontWeights.Bold;
            header.Margin = new Thickness(0, 0, 0, 10);
            section.Blocks.Add(header);

            // Create table
            Table table = new Table();
            table.CellSpacing = 0;
            table.Borders = new Thickness(1);
            table.BorderBrush = Brushes.Gray;

            // Define columns
            table.Columns.Add(new TableColumn { Width = new GridLength(300) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });

            // Add header row
            TableRowGroup headerRow = new TableRowGroup();
            TableRow hr = new TableRow();
            hr.Background = Brushes.LightGray;
            
            Paragraph categoryHeader = new Paragraph(new Run("Category"));
            categoryHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(categoryHeader));
            
            Paragraph salesHeader = new Paragraph(new Run("Sales"));
            salesHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(salesHeader));
            
            headerRow.Rows.Add(hr);
            table.RowGroups.Add(headerRow);

            // Add data rows
            TableRowGroup dataRows = new TableRowGroup();
            foreach (var category in categories.OrderByDescending(c => c.TotalSales))
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(category.CategoryName))));
                
                Paragraph salesPara = new Paragraph(new Run($"{category.TotalSales:C}"));
                salesPara.TextAlignment = TextAlignment.Right;
                row.Cells.Add(new TableCell(salesPara));
                
                dataRows.Rows.Add(row);
            }
            table.RowGroups.Add(dataRows);

            section.Blocks.Add(table);
            document.Blocks.Add(section);
        }

        private void AddDailySalesSection(FlowDocument document, IEnumerable<SalesData> dailySales)
        {
            Section section = new Section();
            Paragraph header = new Paragraph(new Run("Daily Sales"));
            header.FontSize = 18;
            header.FontWeight = FontWeights.Bold;
            header.Margin = new Thickness(0, 0, 0, 10);
            section.Blocks.Add(header);

            // Create table
            Table table = new Table();
            table.CellSpacing = 0;
            table.Borders = new Thickness(1);
            table.BorderBrush = Brushes.Gray;

            // Define columns
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });

            // Add header row
            TableRowGroup headerRow = new TableRowGroup();
            TableRow hr = new TableRow();
            hr.Background = Brushes.LightGray;
            
            Paragraph dateHeader = new Paragraph(new Run("Date"));
            dateHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(dateHeader));
            
            Paragraph ordersHeader = new Paragraph(new Run("Orders"));
            ordersHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(ordersHeader));
            
            Paragraph salesHeader = new Paragraph(new Run("Sales"));
            salesHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(salesHeader));
            
            headerRow.Rows.Add(hr);
            table.RowGroups.Add(headerRow);

            // Add data rows
            TableRowGroup dataRows = new TableRowGroup();
            foreach (var day in dailySales.OrderBy(d => d.Date))
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(day.Date.ToString("d")))));
                
                Paragraph ordersPara = new Paragraph(new Run(day.OrderCount.ToString()));
                ordersPara.TextAlignment = TextAlignment.Center;
                row.Cells.Add(new TableCell(ordersPara));
                
                Paragraph salesPara = new Paragraph(new Run($"{day.TotalSales:C}"));
                salesPara.TextAlignment = TextAlignment.Right;
                row.Cells.Add(new TableCell(salesPara));
                
                dataRows.Rows.Add(row);
            }
            table.RowGroups.Add(dataRows);

            section.Blocks.Add(table);
            document.Blocks.Add(section);
        }

        private void AddMonthlySalesSection(FlowDocument document, IEnumerable<SalesData> monthlySales)
        {
            Section section = new Section();
            Paragraph header = new Paragraph(new Run("Monthly Sales"));
            header.FontSize = 18;
            header.FontWeight = FontWeights.Bold;
            header.Margin = new Thickness(0, 0, 0, 10);
            section.Blocks.Add(header);

            // Create table
            Table table = new Table();
            table.CellSpacing = 0;
            table.Borders = new Thickness(1);
            table.BorderBrush = Brushes.Gray;

            // Define columns
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });

            // Add header row
            TableRowGroup headerRow = new TableRowGroup();
            TableRow hr = new TableRow();
            hr.Background = Brushes.LightGray;
            
            Paragraph monthHeader = new Paragraph(new Run("Month"));
            monthHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(monthHeader));
            
            Paragraph ordersHeader = new Paragraph(new Run("Orders"));
            ordersHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(ordersHeader));
            
            Paragraph salesHeader = new Paragraph(new Run("Sales"));
            salesHeader.FontWeight = FontWeights.Bold;
            hr.Cells.Add(new TableCell(salesHeader));
            
            headerRow.Rows.Add(hr);
            table.RowGroups.Add(headerRow);

            // Add data rows
            TableRowGroup dataRows = new TableRowGroup();
            foreach (var month in monthlySales.OrderBy(m => m.Date))
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(month.Date.ToString("MMM yyyy")))));
                
                Paragraph ordersPara = new Paragraph(new Run(month.OrderCount.ToString()));
                ordersPara.TextAlignment = TextAlignment.Center;
                row.Cells.Add(new TableCell(ordersPara));
                
                Paragraph salesPara = new Paragraph(new Run($"{month.TotalSales:C}"));
                salesPara.TextAlignment = TextAlignment.Right;
                row.Cells.Add(new TableCell(salesPara));
                
                dataRows.Rows.Add(row);
            }
            table.RowGroups.Add(dataRows);

            section.Blocks.Add(table);
            document.Blocks.Add(section);
        }

        public void ExportToExcel<T>(IEnumerable<T> data, string reportTitle)
        {
            try
            {
                // In a real application, this would use a library like EPPlus or NPOI
                // to create an actual Excel file. For this example, we'll just create a CSV.
                
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Export Report",
                    FileName = reportTitle.Replace(":", "").Replace(" ", "_") + ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Write headers
                        var properties = typeof(T).GetProperties();
                        writer.WriteLine(string.Join(",", properties.Select(p => p.Name)));

                        // Write data
                        foreach (var item in data)
                        {
                            var values = properties.Select(p => 
                            {
                                var value = p.GetValue(item);
                                if (value is DateTime date)
                                {
                                    return date.ToString("yyyy-MM-dd");
                                }
                                else if (value is decimal dec)
                                {
                                    return dec.ToString("0.00");
                                }
                                else
                                {
                                    return value?.ToString() ?? "";
                                }
                            });
                            writer.WriteLine(string.Join(",", values));
                        }
                    }

                    MessageBox.Show($"Report exported successfully to {saveFileDialog.FileName}", 
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", 
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}