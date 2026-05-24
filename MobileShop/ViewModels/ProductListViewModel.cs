using MobileShop.Models;
using System.Collections.Generic;

namespace MobileShop.ViewModels
{
    public class ProductListViewModel
    {
        // Data Lists
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Brand> Brands { get; set; } = new List<Brand>();

        // Filter Parameters
        public int? SelectedCategoryId { get; set; }
        public int? SelectedBrandId { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortOrder { get; set; }

        // Price Range Filtering
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Pagination Properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}