using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileShop.Interfaces;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Controllers;

public class CheckoutController : Controller
{
    private readonly IShoppingCartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(
        IShoppingCartService cartService,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<CheckoutController> logger)
    {
        _cartService = cartService;
        _userManager = userManager;
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
        return View(model);
    }
}