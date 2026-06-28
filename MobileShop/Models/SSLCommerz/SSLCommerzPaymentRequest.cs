namespace MobileShop.Models.SSLCommerz;

// This is YOUR internal model.
// You fill this from checkout form data.
// The service converts this into SSLCommerz's exact format.
// This way your controller stays clean.
public class SSLCommerzPaymentRequest
{
    // Unique transaction ID — use your OrderNumber
    public string TranId { get; set; } = "";

    // Order total
    public decimal TotalAmount { get; set; }

    // Currency — use "BDT" for Bangladesh
    public string Currency { get; set; } = "BDT";

    // Customer details
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string CustomerAddress { get; set; } = "";
    public string CustomerCity { get; set; } = "";
    public string CustomerPostcode { get; set; } = "";
    public string CustomerCountry { get; set; } = "";

    // Product details
    public string ProductName { get; set; } = "";
    public string ProductCategory { get; set; } = "Mobile";
}