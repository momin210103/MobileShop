using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Interfaces;
using MobileShop.Models;

namespace MobileShop.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BrandsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;
    public BrandsController(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }
    
    // GET Admin/Bands
    public async Task<IActionResult> Index()
    {
        var brands = await _context.Brands.Include(c => c.Products).ToListAsync();
        return View(brands);
    }
    
    
    public ActionResult Create()
    {
        return View();
    }
    // POST: Admin/Bands/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Brand brand)
    {
        if (ModelState.IsValid)
        {
            if (brand.Logo != null)
            {
                var logoPath = await _fileService.SaveFileAsync(brand.Logo, "images/brand");
                if (!string.IsNullOrEmpty(logoPath))
                {
                    if(!string.IsNullOrEmpty(brand.LogoUrl))
                        _fileService.DeleteFile(brand.LogoUrl);
                    brand.LogoUrl = logoPath;
                        
                }
            }
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Brand created successfully.";
            return RedirectToAction(nameof(Index));
        }

        return View(brand);
    }
    
    
}