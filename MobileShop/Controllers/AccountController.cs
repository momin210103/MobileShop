using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Interfaces;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;  // ADD THIS
        private readonly IShoppingCartService _cartService;
        private readonly IFileService _fileService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,  // ADD THIS PARAMETER)
            IShoppingCartService cartService, IFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;  // ADD THIS
            _cartService = cartService;
            _fileService = fileService;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            // 1. Preserving the intended navigation path
            ViewData["ReturnUrl"] = returnUrl;
            
            // 2. Rendering the onboarding interface
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {

                // Migrate cart
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await _cartService.MigrateCartAsync(user.Id);
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                City = user.City,
                PostalCode = user.PostalCode,
                Country = user.Country,
                ProfileImageUrl = user.ProfileImageUrl
            };

            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View(model);
        }
        
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;
            user.City = model.City;
            user.PostalCode = model.PostalCode;
            user.Country = model.Country;

            if (model.ProfileImage != null)
            {
                var imagePath = await _fileService.SaveFileAsync(model.ProfileImage, "images/profiles");
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                        _fileService.DeleteFile(user.ProfileImageUrl);
                    user.ProfileImageUrl = imagePath;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View(model);
        }
        
        
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var checkwishlist = _context.WishlistItems.Where(w => w.ProductId == productId && w.UserId == user.Id).FirstOrDefault();
            if (checkwishlist == null)
            {
                if (!user.WishlistItems.Any(w => w.ProductId == productId))
                {
                    user.WishlistItems.Add(new WishlistItem
                    {
                        ProductId = productId,
                        UserId = user.Id
                    });
                    await _userManager.UpdateAsync(user);
                }
                return Json(new { success = true, message = "Added to wishlist!" });
            }
            return Json(new { success = true, message = "Already Added!" });
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Wishlist()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var wishlistItems = await _context.WishlistItems
                .Include(w => w.Product)
                .ThenInclude(p => p.Brand)
                .Include(w => w.Product)
                .ThenInclude(p => p.Reviews)
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View(wishlistItems);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Success"] = "If your email is registered, you will receive password reset instructions.";
                return RedirectToAction(nameof(Login));
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, protocol: Request.Scheme);

            // TODO: Send email with reset link

            TempData["Success"] = "If your email is registered, you will receive password reset instructions.";
            return RedirectToAction(nameof(Login));
        }
        
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? code = null)
        {
            if (code == null)
                return BadRequest("A code must be supplied for password reset.");

            return View();
        }
        
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}