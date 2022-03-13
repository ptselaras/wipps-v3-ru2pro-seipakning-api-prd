using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class Permission
    {
        public Permission()
        {
            ModelHasPermissions = new HashSet<ModelHasPermission>();
            RoleHasPermissions = new HashSet<RoleHasPermission>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string GuardName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ModelHasPermission> ModelHasPermissions { get; set; }
        public virtual ICollection<RoleHasPermission> RoleHasPermissions { get; set; }
    }
}
