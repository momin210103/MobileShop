namespace MobileShop.Models.SSLCommerz;

// This class represents what SSLCommerz sends back
// when you initiate a payment request.
// SSLCommerz returns JSON — this class maps to that JSON.
public class SSLCommerzResponse
{
    // "SUCCESS" or "FAILED"
    public string? status { get; set; }

    // If failed, why it failed
    public string? failedreason { get; set; }

    // The URL you redirect your customer to for payment
    // This is the most important field
    public string? GatewayPageURL { get; set; }

    // Session key for this transaction
    public string? sessionkey { get; set; }
}