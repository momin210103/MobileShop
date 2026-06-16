using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CheckoutViewModel model, string? userId);
    Task<Order?> GetOrderByNumberAsync(string orderNumber);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task<bool> ProcessPaymentAsync(int orderId, string transactionId);
    Task<List<Order>> GetUserOrdersAsync(string userId);
    Task<Order?> GetOrderByIdAsync(int orderId);
}