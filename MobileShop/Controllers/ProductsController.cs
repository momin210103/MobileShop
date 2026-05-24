using Microsoft.AspNetCore.Mvc;
using MobileShop.Interfaces;
using MobileShop.ViewModels;

namespace MobileShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        // Injecting the Product Service through the constructor
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Displays a filtered and paginated list of products
        /// </summary>
        public async Task<IActionResult> Index(
            int? categoryId,
            int? brandId,
            string? search,
            string? sort,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            // Mapping URL parameters to our ViewModel
            var filter = new ProductListViewModel
            {
                SelectedCategoryId = categoryId,
                SelectedBrandId = brandId,
                SearchTerm = search,
                SortOrder = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageNumber = page
            };

            // Calling the service to handle the filtering logic
            var result = await _productService.GetProductsAsync(filter);

            return View(result);
        }
        
        /// <summary>
        /// Displays the detailed information for a specific product
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            // 1. Fetch the main product details from the service
            var product = await _productService.GetProductByIdAsync(id);
    
            // 2. Safety Check: If product doesn't exist or is inactive, return 404
            if (product == null)
            {
                return NotFound();
            }

            // 3. Fetch related products (upselling)
            var relatedProducts = await _productService.GetRelatedProductsAsync(id);
    
            // 4. Placeholder for Wishlist logic (can be expanded later)
            bool isInWishlist = false;

            // 5. Build the ViewModel
            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                IsInWishlist = isInWishlist
            };

            return View(viewModel);
        }

    }
}
