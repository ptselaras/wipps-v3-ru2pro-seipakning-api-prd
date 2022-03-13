using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class ModelHasPermission
    {
        public long PermissionId { get; set; }
        public string ModelType { get; set; }
        public long ModelId { get; set; }

        public virtual Permission Permission { get; set; }
    }
}
