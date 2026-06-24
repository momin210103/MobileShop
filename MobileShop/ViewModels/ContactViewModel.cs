using System.ComponentModel.DataAnnotations;

namespace MobileShop.ViewModels;

public class ContactViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Required] [StringLength(200)] public string Subject { get; set; }

    [Required] [StringLength(2000)] public string Message { get; set; }
}