using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShop.Data;

namespace MobileShop.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = "Admin")]

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<IActionResult> Index(string? search, int? categoryId, int? brandId, int page=1)
    {
        
        // Build deferred execution query including relationships
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();
        
        // Apply Search Filtering
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Model.Contains(search));
        }
        
        // Apply Category Filtering
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }
        
        // Apply Brand Filtering
        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }
        // Pagination Settings
        var pageSize = 20;
        var totalItems = await query.CountAsync();
        
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        // Populate Lookup Data and State for the View
        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        ViewBag.Search = search;

        return View(products);

    }

    
}