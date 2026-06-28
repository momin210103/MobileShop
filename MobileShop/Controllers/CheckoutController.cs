using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileShop.Interfaces;
using MobileShop.Models;
using MobileShop.Models.SSLCommerz;
using MobileShop.Services;
using MobileShop.ViewModels;
using Stripe;

namespace MobileShop.Controllers;

public class CheckoutController : Controller
{
    private readonly IShoppingCartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IOrderService _orderService;
    private readonly ISSLCommerzService _sslCommerzService;


    public CheckoutController(
        IShoppingCartService cartService,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IEmailService emailService,
        IOrderService orderService, ISSLCommerzService sslCommerzService)
    {
        _cartService = cartService;
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
        _orderService = orderService;
        _sslCommerzService = sslCommerzService;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync();
        if (cart.CartItems.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction("Index", "ShoppingCart");
        }

        var user = await _userManager.GetUserAsync(User);
        var model = new CheckoutViewModel
        {
            Cart = cart,
            IsAuthenticated = user != null,
            FirstName = user?.FirstName ?? "",
            LastName = user?.LastName ?? "",
            Email = user?.Email ?? "",
            Phone = user?.PhoneNumber ?? "",
            Address = user?.Address ?? "",
            City = user?.City ?? "",
            PostalCode = user?.PostalCode ?? "",
            Country = user?.Country ?? "Pakistan"
        };

        ViewBag.CartItemCount = cart.ItemCount;
        ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(CheckoutViewModel model)
    {
        var cart = await _cartService.GetCartAsync();
        if (cart.CartItems.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction("Index", "ShoppingCart");
        }

        model.Cart = cart;

        if (!ModelState.IsValid)
        {
            ViewBag.CartItemCount = cart.ItemCount;
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            return View("Index", model);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Create order
            var order = await _orderService.CreateOrderAsync(model, userId);

            // Process payment based on method
            if (model.PaymentMethod == Models.PaymentMethod.Stripe)
            {
                return await ProcessStripePayment(order, model);
            }
            else if (model.PaymentMethod == Models.PaymentMethod.CashOnDelivery)
            {
                order.PaymentStatus = PaymentStatus.Pending;
                await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Pending);
            }
            else if (model.PaymentMethod == Models.PaymentMethod.SSLCommerz)
            {
                return await ProcessSSLCommerzPayment(order, model);
            }
            else
            {
                // For other payment methods, mark as paid for demo
                await _orderService.ProcessPaymentAsync(order.Id, $"DEMO-{Guid.NewGuid()}");
            }

            // Clear cart
            await _cartService.ClearCartAsync();

            // Send confirmation email
            await _emailService.SendOrderConfirmationAsync(model.Email, order.OrderNumber, order.TotalAmount);

            return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred during checkout. Please try again.");
            ViewBag.CartItemCount = cart.ItemCount;
            return View("Index", model);
        }
    }


    private async Task<IActionResult> ProcessStripePayment(Order order, CheckoutViewModel model)
    {
        try
        {
            var options = new ChargeCreateOptions
            {
                Amount = (long)(order.TotalAmount * 100),
                Currency = "PKR",
                Description = $"Order {order.OrderNumber}",
                Source = model.StripeToken
            };

            var service = new ChargeService();
            var charge = await service.CreateAsync(options);

            if (charge.Status == "succeeded")
            {
                await _orderService.ProcessPaymentAsync(order.Id, charge.Id);
                await _cartService.ClearCartAsync();
                await _emailService.SendOrderConfirmationAsync(model.Email, order.OrderNumber, order.TotalAmount);

                return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
            }
            else
            {
                TempData["Error"] = "Payment failed. Please try again.";
                return RedirectToAction("Index");
            }
        }
        catch (StripeException ex)
        {
            TempData["Error"] = $"Payment error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }


    public async Task<IActionResult> Confirmation(string orderNumber)
    {
        var order = await _orderService.GetOrderByNumberAsync(orderNumber);
        if (order == null) return NotFound();

        var viewModel = new OrderConfirmationViewModel
        {
            Order = order,
            OrderItems = order.OrderItems.ToList()
        };

        ViewBag.CartItemCount = 0;
        return View(viewModel);
    }


    public async Task<IActionResult> TrackOrder(string? orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) return View();

        var order = await _orderService.GetOrderByNumberAsync(orderNumber);
        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return View();
        }

        return View(order);
    }

    private async Task<IActionResult> ProcessSSLCommerzPayment(Order order, CheckoutViewModel model)
    {
        var request = new SSLCommerzPaymentRequest
        {
            TranId = order.OrderNumber, // your unique ID sent to SSLCommerz
            TotalAmount = order.TotalAmount,
            Currency = "BDT",
            CustomerName = $"{model.FirstName} {model.LastName}",
            CustomerEmail = model.Email,
            CustomerPhone = model.Phone,
            CustomerAddress = model.Address,
            CustomerCity = model.City,
            CustomerPostcode = model.PostalCode,
            CustomerCountry = model.Country,
            ProductName = $"Order {order.OrderNumber}",
            ProductCategory = "Mobile"
        };
        var sslResponse = await _sslCommerzService.InitiatePaymentAsync(request);
        if (sslResponse.status == "SUCCESS"
            && !string.IsNullOrEmpty(sslResponse.GatewayPageURL))
            // Send customer to SSLCommerz payment page
            // They will pay here, then SSLCommerz calls your SSLSuccess
            return Redirect(sslResponse.GatewayPageURL);

        // Initiation failed — go back to checkout with error
        TempData["Error"] = $"Payment initiation failed: {sslResponse.failedreason}";
        return RedirectToAction("Index");
    }


    // ────────────────────────────────────────────────────────────────
// PUBLIC: SSLCommerz calls this after successful payment
// Must be [HttpPost] — SSLCommerz sends POST not GET
// ────────────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> SSLSuccess(SSLCommerzIPN ipnData)
    {
        // SECURITY: Validate with SSLCommerz server
        // This prevents hackers from faking a success POST
        var isValid = await _sslCommerzService.ValidatePaymentAsync(ipnData.val_id!);

        if (!isValid)
        {
            TempData["Error"] = "Payment could not be verified. Contact support.";
            return RedirectToAction("Index");
        }

        // Find order using tran_id = your OrderNumber you sent earlier
        var order = await _orderService.GetOrderByNumberAsync(ipnData.tran_id!);
        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction("Index");
        }

        // Mark order as paid using existing ProcessPaymentAsync
        // Store val_id in your TransactionId column — same as Stripe stores charge.Id
        await _orderService.ProcessPaymentAsync(order.Id, ipnData.val_id!);

        // Now safe to clear the cart
        await _cartService.ClearCartAsync();

        // Send confirmation email
        // Try order user email first, fall back to IPN customer email
        var email = order.User?.Email
                    ?? ipnData.cus_email
                    ?? "";

        await _emailService.SendOrderConfirmationAsync(
            email,
            order.OrderNumber,
            order.TotalAmount);

        return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
    }


// ────────────────────────────────────────────────────────────────
// PUBLIC: SSLCommerz calls this if payment fails
// ────────────────────────────────────────────────────────────────
    [HttpPost]
    public IActionResult SSLFail(SSLCommerzIPN ipnData)
    {
        TempData["Error"] = "Payment failed. Please try again or choose another method.";
        return RedirectToAction("Index");
    }


// ────────────────────────────────────────────────────────────────
// PUBLIC: SSLCommerz calls this if customer cancels
// ────────────────────────────────────────────────────────────────
    [HttpPost]
    public IActionResult SSLCancel(SSLCommerzIPN ipnData)
    {
        TempData["Error"] = "Payment was cancelled.";
        return RedirectToAction("Index");
    }


// ────────────────────────────────────────────────────────────────
// PUBLIC: SSLCommerz server-to-server background notification
// Called even if customer's browser closes before SSLSuccess loads
// This is your safety net
// ────────────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> SSLIPN(SSLCommerzIPN ipnData)
    {
        if (ipnData.status == "VALID" || ipnData.status == "VALIDATED")
        {
            var order = await _orderService.GetOrderByNumberAsync(ipnData.tran_id!);

            // Only process if not already marked paid by SSLSuccess
            if (order != null && order.PaymentStatus != PaymentStatus.Paid)
                await _orderService.ProcessPaymentAsync(order.Id, ipnData.val_id!);
        }

        // SSLCommerz expects HTTP 200 — if you return error it keeps retrying
        return Ok();
    }
}