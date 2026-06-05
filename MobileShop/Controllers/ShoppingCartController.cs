using Microsoft.AspNetCore.Mvc;

namespace MobileShop.Controllers;

public class ShoppingCartController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}