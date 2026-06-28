namespace MobileShop.Models.SSLCommerz;

// IPN = Instant Payment Notification
// After customer pays, SSLCommerz sends these fields
// as a POST request to your SuccessUrl / FailUrl / CancelUrl.
// ASP.NET automatically maps form fields to this class.
public class SSLCommerzIPN
{
    // YOUR order number that you sent to SSLCommerz
    // This is how you find the order in your database
    public string? tran_id { get; set; }

    // SSLCommerz's own validation ID
    // You use this to verify payment was real
    public string? val_id { get; set; }

    // Amount paid — comes as string like "500.00"
    public string? amount { get; set; }

    // Currency — "BDT"
    public string? currency { get; set; }

    // Payment status from SSLCommerz
    public string? status { get; set; }

    // Bank or bKash transaction reference
    // This is what appears on customer's bKash app
    public string? bank_tran_id { get; set; }

    // Payment type — "bKash", "VISA", "Nagad" etc.
    public string? card_type { get; set; }

    // Customer info SSLCommerz sends back
    public string? cus_name { get; set; }
    public string? cus_email { get; set; }
    public string? cus_phone { get; set; }
}