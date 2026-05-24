using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Interfaces
{
    public interface IProductService
    {
        // Fetches a single product by its primary key
        Task<Product?> GetProductByIdAsync(int id);

        // Fetches a list of similar products based on Category or Brand
        // Default count is set to 4 for a standard UI row
        Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4);
        Task<List<Product>> GetFeaturedProductsAsync(int count = 8);
        Task<List<Product>> GetNewArrivalsAsync(int count = 8);
        Task<List<Product>> GetBestsellersAsync(int count = 8);
        Task<List<Brand>> GetBrandsAsync(int count = 6);
        // Asynchronously gets a filtered, sorted, and paginated list of products
        Task<ProductListViewModel> GetProductsAsync(ProductListViewModel filter);
        
        
    }

    
}