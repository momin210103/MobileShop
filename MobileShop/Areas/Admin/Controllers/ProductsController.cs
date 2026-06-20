using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Interfaces;
using MobileShop.Models;

namespace MobileShop.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = "Admin")]

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;
    public ProductsController(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
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
    
    
    // GET: Admin/Products/Create
    public async Task<IActionResult> Create()
    {
        // Populate lookup collections for lookups/drop-downs before loading the form
        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();
    
        return View();
    }
    
    // POST: Admin/Products/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Name,Model,CategoryId,BrandId,OriginalPrice,SalePrice,StockQuantity,ShortDescription,Description,IsActive,IsFeatured,IsNewArrival,IsBestseller")] Product product, List<IFormFile> images)
{
    // Remove EF validation errors for complex navigation properties to prevent false validation failures
    ModelState.Remove("Category");
    ModelState.Remove("Brand");
    ModelState.Remove("ProductImages");
    ModelState.Remove("Specifications");
    ModelState.Remove("Reviews");
    ModelState.Remove("OrderItems");
    ModelState.Remove("WishlistItems");
    
    if (ModelState.IsValid)
    {
        // Save the primary placeholder/Main image if uploaded
        if (images.Count > 0)
        {
            product.MainImageUrl = await _fileService.SaveFileAsync(images[0], "images/products");
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(); // Generates product.Id for relational storage

        // Loop through and assign additional gallery references
        for (int i = 1; i < images.Count; i++)
        {
            var imagePath = await _fileService.SaveFileAsync(images[i], "images/products");
            _context.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = imagePath,
                DisplayOrder = i
            });
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Repopulate lookup configurations if validation checks fail to prevent form rendering breaking
    ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
    ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();
    return View(product);
}

    
}