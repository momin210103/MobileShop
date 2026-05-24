using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetFeaturedProductsAsync(int count = 8);
        Task<List<Product>> GetNewArrivalsAsync(int count = 8);
        Task<List<Product>> GetBestsellersAsync(int count = 8);
        Task<List<Brand>> GetBrandsAsync(int count = 6);
        // Asynchronously gets a filtered, sorted, and paginated list of products
        Task<ProductListViewModel> GetProductsAsync(ProductListViewModel filter);
    }

    
}