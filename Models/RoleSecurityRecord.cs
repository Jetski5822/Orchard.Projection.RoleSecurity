using System.Collections.Generic;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Roles.Models;

namespace Orchard.Projection.RoleSecurity.Models {
    public class RoleSecurityRecord {
        public RoleSecurityRecord() {
            RolesSecuritys = new List<RolesSecuritysRecord>();
        }

        public virtual int Id { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
        public virtual bool AnonymousCanView {get;set;}
        public virtual PermissionRecord Permission { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<RolesSecuritysRecord> RolesSecuritys { get; set; }
    }
}