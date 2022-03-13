using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class AreaEmail
    {
        public long Id { get; set; }
        public long AreaId { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Area Area { get; set; }
    }
}
