namespace MOAS.Models
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using MOAS.Repositories;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class User
    {
        public int UserID { get; set; }
        [Display(Name = "User Name"), Required, StringLength(100)]
        public string UserName { get; set; }
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        public bool Exist { get; set; }
        public EUserType UserType { get; set; }
        public DateTime LastUpdate { get; set; }
        //public DateTime CreateDate { get; set; }    
        public string HostName { get; set; }

        [StringLength(100), DataType(DataType.Password)]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match."), DataType(DataType.Password), Display(Name = "Confirm password"), NotMapped]
        public string ConfirmPassword { get; set; }

        public User()
        {
            this.Roles = new HashSet<Role>();
        }

        public string DisplayText
        {
            get
            {
                return (this.UserName + "-" + this.FullName);
            }
        }
        public virtual ICollection<Role> Roles { get; set; }

        public string SearchText
        {
            get
            {
                return ($"{UserID} {UserName} {FullName} {Email}");
            }
        }
 
       
    }
}

