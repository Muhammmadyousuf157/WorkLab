using System.ComponentModel.DataAnnotations;

namespace WorkLabWeb.Areas.Users.Models
{
	public class SignInViewModel
	{
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter your email address")]
        [EmailAddress(ErrorMessage = "Email Address is invalid")]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
