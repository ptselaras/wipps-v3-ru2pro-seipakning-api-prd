using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class ModelHasRole
    {
        public long RoleId { get; set; }
        public string ModelType { get; set; }
        public long ModelId { get; set; }

        public virtual Role Role { get; set; }
    }
}
