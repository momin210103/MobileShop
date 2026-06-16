using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Interfaces;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<AdminViewModel.DashboardViewModel> GetDashboardDataAsync()
    {
            var now = DateTime.Now;
            var thirtyDaysAgo = now.AddDays(-30);

            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Users.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount);
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);
            var lowStockProducts = await _context.Products
                .CountAsync(p => p.StockQuantity <= 10);

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new AdminViewModel.RecentOrderViewModel
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User != null ? o.User.FullName : "Guest",
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            var topProducts = _context.OrderItems
                .Include(oi => oi.Product)
                .AsEnumerable()
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new AdminViewModel.TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(tp => tp.Revenue)
                .Take(5)
                .ToList();

            var monthlySales = new List<AdminViewModel.MonthlySalesViewModel>();
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var sales = await _context.Orders
                    .Where(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd && o.PaymentStatus == PaymentStatus.Paid)
                    .SumAsync(o => o.TotalAmount);

                var orders = await _context.Orders
                    .CountAsync(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd);

                monthlySales.Add(new AdminViewModel.MonthlySalesViewModel
                {
                    Month = month.ToString("MMM yyyy"),
                    Sales = sales,
                    Orders = orders
                });
            }

            return new AdminViewModel.DashboardViewModel
            {
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders,
                LowStockProducts = lowStockProducts,
                RecentOrders = recentOrders,
                TopProducts = topProducts,
                MonthlySales = monthlySales
            };
        
    }
}