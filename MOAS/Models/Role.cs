using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MOAS.Models
{
    public class Role
    {
        public int RoleID { get; set; }
        [Required]
        public string Name { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        [StringLength(100)]
        public string ModuleName { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public Role()
        {
            Users = new HashSet<User>();
        }
    }
}
