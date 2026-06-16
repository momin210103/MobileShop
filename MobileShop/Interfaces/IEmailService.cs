namespace MobileShop.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendOrderConfirmationAsync(string to, string orderNumber, decimal total);
    Task SendOrderStatusUpdateAsync(string to, string orderNumber, string status);
}