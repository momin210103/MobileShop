using MobileShop.ViewModels;

namespace MobileShop.Interfaces;

public interface IShoppingCartService
{
    Task AddToCartAsync(int productId, int quantity = 1);
    Task<int> GetCartItemCountAsync();
    Task<ShoppingCartViewModel> GetCartAsync();
    Task UpdateCartItemAsync(int cartItemId, int quantity);
    Task RemoveFromCartAsync(int cartItemId);
    Task ClearCartAsync();
    Task MigrateCartAsync(string userId);
}