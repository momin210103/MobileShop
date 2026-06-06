using Microsoft.AspNetCore.Mvc;

namespace MobileShop.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}