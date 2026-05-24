using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Interfaces;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Services
{
    public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetBestsellersAsync(int count = 8)
    {
        return await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Reviews)
            .Where(p => p.IsBestseller && p.IsActive)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Product>> GetFeaturedProductsAsync(int count = 8)
    {
        return await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Reviews)
            .Where(p => p.IsFeatured && p.IsActive)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Product>> GetNewArrivalsAsync(int count = 8)
    {
        return await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Reviews)
            .Where(p => p.IsNewArrival && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Brand>> GetBrandsAsync(int count = 6)
    {
        return await _context.Brands.Where(b => b.IsActive == true).Take(count).ToListAsync();
    }
    
    public async Task<ProductListViewModel> GetProductsAsync(ProductListViewModel filter)
{
    // 1. Initialize query with related data
    var query = _context.Products
        .Include(p => p.Brand)
        .Include(p => p.Category)
        .Include(p => p.Reviews)
        .Where(p => p.IsActive)
        .AsQueryable();

    // 2. Apply Dynamic Filters
    if (filter.SelectedCategoryId.HasValue)
        query = query.Where(p => p.CategoryId == filter.SelectedCategoryId.Value);

    if (filter.SelectedBrandId.HasValue)
        query = query.Where(p => p.BrandId == filter.SelectedBrandId.Value);

    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        query = query.Where(p => p.Name.Contains(filter.SearchTerm) ||
                                 p.Description.Contains(filter.SearchTerm) ||
                                 p.Brand.Name.Contains(filter.SearchTerm));

    if (filter.MinPrice.HasValue)
        query = query.Where(p => p.SalePrice >= filter.MinPrice.Value);

    if (filter.MaxPrice.HasValue)
        query = query.Where(p => p.SalePrice <= filter.MaxPrice.Value);

    // 3. Apply Sorting using C# Switch Expression
    query = filter.SortOrder?.ToLower() switch
    {
        "price_asc" => query.OrderBy(p => p.SalePrice),
        "price_desc" => query.OrderByDescending(p => p.SalePrice),
        "name_asc" => query.OrderBy(p => p.Name),
        "name_desc" => query.OrderByDescending(p => p.Name),
        "newest" => query.OrderByDescending(p => p.CreatedAt),
        _ => query.OrderByDescending(p => p.CreatedAt)
    };

    // 4. Calculate Pagination Stats
    filter.TotalCount = await query.CountAsync();
    filter.TotalPages = (int)Math.Ceiling(filter.TotalCount / (double)filter.PageSize);

    // 5. Execute Query with Skip/Take
    filter.Products = await query
        .Skip((filter.PageNumber - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .ToListAsync();

    // 6. Populate filter dropdowns
    filter.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
    filter.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();

    return filter;
}

}
    
}

