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


    public async Task<IActionResult> Index(string? search, int? categoryId, int? brandId, int page = 1)
    {
        // Build deferred execution query including relationships
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();

        // Apply Search Filtering
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Model.Contains(search));

        // Apply Category Filtering
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);

        // Apply Brand Filtering
        if (brandId.HasValue) query = query.Where(p => p.BrandId == brandId.Value);
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
    public async Task<IActionResult> Create(
        [Bind(
            "Name,Model,CategoryId,BrandId,OriginalPrice,SalePrice,StockQuantity,ShortDescription,Description,IsActive,IsFeatured,IsNewArrival,IsBestseller")]
        Product product, List<IFormFile> images)
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
            if (images.Count > 0) product.MainImageUrl = await _fileService.SaveFileAsync(images[0], "images/products");

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // Generates product.Id for relational storage

            // Loop through and assign additional gallery references
            for (var i = 1; i < images.Count; i++)
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


    // GET: Admin/Products/GetLatestProductId
    [HttpGet]
    public async Task<IActionResult> GetLatestProductId()
    {
        // Fetch only the ID of the most recently inserted product to minimize database transmission payload
        var latest = await _context.Products
            .OrderByDescending(p => p.Id)
            .Select(p => new { id = p.Id })
            .FirstOrDefaultAsync();

        // Return the result as a JSON object, defaulting to an ID of 0 if the table is currently empty
        return Json(latest ?? new { id = 0 });
    }

    // POST: Admin/Products/AddSpecification
    [HttpPost]
    public async Task<IActionResult> AddSpecification(int productId, string name, string value, string? groupName)
    {
        // Initialize a new specification entity mapping technical attributes
        var spec = new ProductSpecification
        {
            ProductId = productId,
            Name = name,
            Value = value,
            GroupName = groupName // Optional parameter enabling dynamic categorization groupings
        };

        _context.ProductSpecifications.Add(spec);
        await _context.SaveChangesAsync(); // Persists and generates the database-level spec.Id

        // Return an anonymous JSON payload indicating successful execution state alongside the record identity
        return Json(new { success = true, id = spec.Id });
    }

// Helper method used to verify product existence within the master context
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }


    // GET: Admin/Products/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        // Eagerly load relational images and specifications lists to populate the editing dashboard view tabs
        var product = await _context.Products
            .Include(p => p.ProductImages)
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Return a 404 status error if the target tracking identifier is invalid or missing
        if (product == null)
            return NotFound();

        // Repopulate selective select list structures to maintain drop-down input integrity
        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();

        return View(product);
    }

    // POST: Admin/Products/RemoveSpecification/5
    [HttpPost]
    public async Task<IActionResult> RemoveSpecification(int id)
    {
        // Locate the matching technical requirement entry inside the tracking context asynchronously
        var spec = await _context.ProductSpecifications.FindAsync(id);

        // Safely verify existence to prevent targeting exceptions during state removal phases
        if (spec != null)
        {
            _context.ProductSpecifications.Remove(spec);
            await _context.SaveChangesAsync(); // Commit structural data modifications directly to the database
        }

        // Return a lightweight status verification result payload to confirm data pipeline clearance
        return Json(new { success = true });
    }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,Name,Model,CategoryId,BrandId,OriginalPrice,SalePrice,StockQuantity,ShortDescription,Description,IsActive,IsFeatured,IsNewArrival,IsBestseller")]
    Product product,
    List<IFormFile>? images)
{
    if (id != product.Id)
        return NotFound();

    ModelState.Remove("Brand");
    ModelState.Remove("Category");
    ModelState.Remove("ProductImages");
    ModelState.Remove("Specifications");
    ModelState.Remove("Reviews");
    ModelState.Remove("OrderItems");
    ModelState.Remove("WishlistItems");

    if (!ModelState.IsValid)
    {
        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .ToListAsync();

        ViewBag.Brands = await _context.Brands
            .Where(b => b.IsActive)
            .ToListAsync();

        return View(product);
    }

    var existingProduct = await _context.Products
        .Include(p => p.ProductImages)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (existingProduct == null)
        return NotFound();

    try
    {
        // Update fields
        existingProduct.Name = product.Name;
        existingProduct.Model = product.Model;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.BrandId = product.BrandId;
        existingProduct.OriginalPrice = product.OriginalPrice;
        existingProduct.SalePrice = product.SalePrice;
        existingProduct.StockQuantity = product.StockQuantity;
        existingProduct.ShortDescription = product.ShortDescription;
        existingProduct.Description = product.Description;
        existingProduct.IsActive = product.IsActive;
        existingProduct.IsFeatured = product.IsFeatured;
        existingProduct.IsNewArrival = product.IsNewArrival;
        existingProduct.IsBestseller = product.IsBestseller;
        existingProduct.UpdatedAt = DateTime.Now;

        // Main image replacement
        if (images != null && images.Count > 0 && images[0].Length > 0)
        {
            if (!string.IsNullOrEmpty(existingProduct.MainImageUrl))
            {
                _fileService.DeleteFile(existingProduct.MainImageUrl);
            }

            existingProduct.MainImageUrl =
                await _fileService.SaveFileAsync(
                    images[0],
                    "images/products");
        }

        // Additional images
        if (images != null && images.Count > 1)
        {
            for (int i = 1; i < images.Count; i++)
            {
                if (images[i].Length == 0)
                    continue;

                var imagePath = await _fileService.SaveFileAsync(
                    images[i],
                    "images/products");

                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = existingProduct.Id,
                    ImageUrl = imagePath,
                    DisplayOrder = i
                });
            }
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Product updated successfully.";

        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        TempData["Error"] = ex.Message;
    }

    ViewBag.Categories = await _context.Categories
        .Where(c => c.IsActive)
        .ToListAsync();

    ViewBag.Brands = await _context.Brands
        .Where(b => b.IsActive)
        .ToListAsync();

    return View(existingProduct);
}

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductImages)
            .Include(p => p.Specifications)
            .Include(p => p.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        // Soft delete
        product.IsActive = false;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}