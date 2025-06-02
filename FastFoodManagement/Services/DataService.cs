using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using FastFoodManagement.Data;
using FastFoodManagement.Data.Models;

namespace FastFoodManagement.Services
{
    public class DataService
    {
        private readonly DatabaseContext _context;

        public DataService()
        {
            _context = new DatabaseContext();
        }

        #region Category Methods
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Product Methods
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return false;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Order Methods
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<bool> AddOrderAsync(Order order)
        {
            try
            {
                // Calculate subtotals and total
                foreach (var item in order.OrderItems)
                {
                    item.Subtotal = item.Quantity * item.UnitPrice;
                }
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Subtotal);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                // Recalculate subtotals and total
                foreach (var item in order.OrderItems)
                {
                    item.Subtotal = item.Quantity * item.UnitPrice;
                }
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Subtotal);

                _context.Entry(order).State = EntityState.Modified;
                
                // Handle order items
                foreach (var item in order.OrderItems)
                {
                    if (item.OrderItemId == 0)
                    {
                        // New item
                        _context.Entry(item).State = EntityState.Added;
                    }
                    else
                    {
                        // Existing item
                        _context.Entry(item).State = EntityState.Modified;
                    }
                }

                // Handle deleted items
                var existingItems = await _context.Set<OrderItem>()
                    .Where(oi => oi.OrderId == order.OrderId)
                    .ToListAsync();
                
                foreach (var existingItem in existingItems)
                {
                    if (!order.OrderItems.Any(oi => oi.OrderItemId == existingItem.OrderItemId))
                    {
                        _context.Entry(existingItem).State = EntityState.Deleted;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                    return false;

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Report Methods
        public async Task<Dictionary<string, decimal>> GetSalesByProductAsync(DateTime startDate, DateTime endDate)
        {
            var orderItems = await _context.Set<OrderItem>()
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .ToListAsync();

            return orderItems
                .GroupBy(oi => oi.Product.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(oi => oi.Subtotal)
                );
        }

        public async Task<Dictionary<string, decimal>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate)
        {
            var orderItems = await _context.Set<OrderItem>()
                .Include(oi => oi.Product.Category)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .ToListAsync();

            return orderItems
                .GroupBy(oi => oi.Product.Category.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(oi => oi.Subtotal)
                );
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetOrderCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .CountAsync(o => o.OrderDate >= startDate && o.OrderDate <= endDate);
        }
        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}