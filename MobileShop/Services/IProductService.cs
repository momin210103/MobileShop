using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;

namespace MobileShop.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetFeaturedProductsAsync(int count = 8);
        Task<List<Product>> GetNewArrivalsAsync(int count = 8);
        Task<List<Product>> GetBestsellersAsync(int count = 8);
        Task<List<Brand>> GetBrandsAsync(int count = 6);
    }

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
    }
}