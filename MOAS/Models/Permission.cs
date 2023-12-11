using System.ComponentModel.DataAnnotations.Schema;

namespace MOAS.Models
{
    public class Permission
    {
        [ForeignKey("Roles")]
        public int RoleID { get; set; }
        public virtual Role Roles { get; set; }
        public bool Status { get; set; }
    }
}
