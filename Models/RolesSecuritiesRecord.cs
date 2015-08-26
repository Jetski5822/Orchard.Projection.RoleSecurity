using Orchard.Roles.Models;

namespace Orchard.Projection.RoleSecurity.Models {
    public class RolesSecuritiesRecord {
        public virtual int Id { get; set; }
        public virtual RoleRecord Role { get; set; }
        public virtual RoleSecurityRecord RoleSecurity { get; set; }
    }
}