namespace MobileShop.ViewModels;

public class AdminViewModel
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
        public List<TopProductViewModel> TopProducts { get; set; } = new List<TopProductViewModel>();
        public List<MonthlySalesViewModel> MonthlySales { get; set; } = new List<MonthlySalesViewModel>();
    }
    
    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
    }
    
    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }
    
    public class MonthlySalesViewModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public int Orders { get; set; }
    }
}