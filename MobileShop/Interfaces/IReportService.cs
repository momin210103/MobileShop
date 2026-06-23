using MobileShop.ViewModels;

namespace MobileShop.Interfaces;

public interface IReportService
{
    Task<AdminViewModel.DashboardViewModel> GetDashboardDataAsync();
    Task<AdminViewModel.SalesReportViewModel> GetSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<AdminViewModel.InventoryReportViewModel> GetInventoryReportAsync();
    Task<List<AdminViewModel.TopProductViewModel>> GetTopSellingProductsAsync(int count = 10);
}