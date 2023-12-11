using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MOAS.Models.VM
{
    public class ChangePasswordModel
    {
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match."), DataType(DataType.Password), Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }

        [Required, Display(Name = "New password"), StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4), DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Current password")]
        public string OldPassword { get; set; }
    }

    public class LogOnModel
    {
        
        [Display(Name = "Password"), Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Display(Name = "User name"), Required]
        public string UserName { get; set; }
    }
}
