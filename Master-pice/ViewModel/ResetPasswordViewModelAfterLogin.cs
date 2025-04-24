using System.ComponentModel.DataAnnotations;

namespace Master_pice.ViewModel
{
    public class ResetPasswordViewModelAfterLogin
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
