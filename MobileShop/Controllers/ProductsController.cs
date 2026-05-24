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

    }
}
