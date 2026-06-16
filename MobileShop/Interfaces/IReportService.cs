using MobileShop.ViewModels;

namespace MobileShop.Interfaces;

public interface IReportService
{
    Task<AdminViewModel.DashboardViewModel> GetDashboardDataAsync();
}