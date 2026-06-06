using System.ComponentModel.DataAnnotations;

namespace MobileShop.ViewModels;

public class ProfileViewModel
{
    [Required]
    [Display(Name = "First Name")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(20)]
    [Display(Name = "Postal Code")]
    public string? PostalCode { get; set; }

    [StringLength(50)]
    public string? Country { get; set; }

    [Display(Name = "Profile Image")]
    public IFormFile? ProfileImage { get; set; }

    public string? ProfileImageUrl { get; set; }
}