using System.ComponentModel.DataAnnotations;

namespace WorkLabWeb.Areas.Users.Models
{
    public class ResetPasswordViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Minimum 8 characters, Maximum 100 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Confirm password does not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}