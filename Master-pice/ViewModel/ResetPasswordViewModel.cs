using System.ComponentModel.DataAnnotations;

namespace Master_pice.ViewModel
{
    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }

}
