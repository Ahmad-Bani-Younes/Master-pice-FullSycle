using System.ComponentModel.DataAnnotations;

namespace Master_pice.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Company
    {
        public int CompanyId { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Password")]
        public string PasswordHash { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Business Category")]
        public string Category { get; set; }

        public string? LogoPath { get; set; }

        public string? BusinessLicensePath { get; set; }

        public List<Laptop>? Laptops { get; set; }
        public List<PC>? PCs { get; set; }
        public List<PCPart>? PCParts { get; set; }
    }

}
