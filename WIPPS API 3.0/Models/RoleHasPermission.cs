using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class RoleHasPermission
    {
        public long PermissionId { get; set; }
        public long RoleId { get; set; }

        public virtual Permission Permission { get; set; }
        public virtual Role Role { get; set; }
    }
}
