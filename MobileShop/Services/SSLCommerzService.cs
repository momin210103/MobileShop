using MobileShop.Models.SSLCommerz;
using Newtonsoft.Json;

namespace MobileShop.Services;

// Interface — defines what this service can do
// Your controller depends on this, not the concrete class
// This makes it easy to test and swap later
public interface ISSLCommerzService
{
    Task<SSLCommerzResponse> InitiatePaymentAsync(SSLCommerzPaymentRequest request);
    Task<bool> ValidatePaymentAsync(string valId);
}

public class SSLCommerzService : ISSLCommerzService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    // ASP.NET injects IConfiguration and HttpClient automatically
    public SSLCommerzService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<SSLCommerzResponse> InitiatePaymentAsync(SSLCommerzPaymentRequest request)
    {
        // Pick sandbox or live URL based on config
        var isSandbox = _config.GetValue<bool>("SSLCommerz:IsSandbox");
        var url = isSandbox
            ? _config["SSLCommerz:SandboxUrl"]
            : _config["SSLCommerz:LiveUrl"];

        // Build form data — SSLCommerz expects these exact field names
        // Think of this like filling out an HTML form and submitting it
        var postData = new Dictionary<string, string>
        {
            // Who you are
            { "store_id", _config["SSLCommerz:StoreId"]! },
            { "store_passwd", _config["SSLCommerz:StorePassword"]! },

            // Order info
            { "total_amount", request.TotalAmount.ToString("F2") },
            { "currency", request.Currency },
            { "tran_id", request.TranId },

            // Where to send customer after payment
            { "success_url", _config["SSLCommerz:SuccessUrl"]! },
            { "fail_url", _config["SSLCommerz:FailUrl"]! },
            { "cancel_url", _config["SSLCommerz:CancelUrl"]! },
            { "ipn_url", _config["SSLCommerz:IPNUrl"]! },

            // Customer info
            { "cus_name", request.CustomerName },
            { "cus_email", request.CustomerEmail },
            { "cus_phone", request.CustomerPhone },
            { "cus_add1", request.CustomerAddress },
            { "cus_city", request.CustomerCity },
            { "cus_postcode", request.CustomerPostcode },
            { "cus_country", request.CustomerCountry },

            // Product info — required by SSLCommerz
            { "product_name", request.ProductName },
            { "product_category", request.ProductCategory },
            { "product_profile", "general" },

            // Shipping info — required even if same as billing
            { "shipping_method", "NO" },
            { "num_of_item", "1" },
            { "ship_name", request.CustomerName },
            { "ship_add1", request.CustomerAddress },
            { "ship_city", request.CustomerCity },
            { "ship_postcode", request.CustomerPostcode },
            { "ship_country", request.CustomerCountry }
        };

        // Send as form POST — like submitting an HTML form
        var content = new FormUrlEncodedContent(postData);
        var response = await _httpClient.PostAsync(url, content);

        // Read the JSON response
        var json = await response.Content.ReadAsStringAsync();

        // Convert JSON text into C# object
        return JsonConvert.DeserializeObject<SSLCommerzResponse>(json)
               ?? new SSLCommerzResponse { status = "FAILED", failedreason = "Empty response" };
    }

    // STEP 4 OF FLOW: Double-check payment was real (security)
    // Never trust what SSLCommerz sends to your success URL alone
    // Always verify server-to-server

    public async Task<bool> ValidatePaymentAsync(string valId)
    {
        var isSandbox = _config.GetValue<bool>("SSLCommerz:IsSandbox");

        // Pick correct validation URL
        var validationUrl = isSandbox
            ? _config["SSLCommerz:ValidationSandboxUrl"]
            : _config["SSLCommerz:ValidationLiveUrl"];

        // Build validation request URL
        var url = $"{validationUrl}" +
                  $"?val_id={valId}" +
                  $"&store_id={_config["SSLCommerz:StoreId"]}" +
                  $"&store_passwd={_config["SSLCommerz:StorePassword"]}" +
                  $"&v=1&format=json";

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        // Use dynamic since we only need the status field
        dynamic? result = JsonConvert.DeserializeObject(json);
        if (result == null) return false;

        string status = result.status ?? "";

        // VALID = payment confirmed for first time
        // VALIDATED = already verified before (IPN already fired)
        // Both mean the payment is genuine
        return status == "VALID" || status == "VALIDATED";
    }
}