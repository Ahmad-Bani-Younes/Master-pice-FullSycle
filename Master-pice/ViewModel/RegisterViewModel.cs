using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Master_pice.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string Phone { get; set; }

        public string UserType { get; set; } = "Customer";

        [Display(Name = "Profile Image")]
        public IFormFile ProfileImage { get; set; }

        [Required]
        public string Region { get; set; }

        public List<SelectListItem> RegionOptions { get; set; } = new List<SelectListItem>();

    }
}
