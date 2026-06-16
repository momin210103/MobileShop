using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileShop.Data;
using MobileShop.Interfaces;
using MobileShop.Models;

namespace MobileShop.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = "Admin")]

public class DashboardController : Controller
{
  
    private readonly IReportService _reportService;

    

    public DashboardController(
        IReportService reportService,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _reportService = reportService;

    }

    public async Task<IActionResult> Index()
    {
        var dashboardData = await _reportService.GetDashboardDataAsync();
        return View(dashboardData);
    }
}