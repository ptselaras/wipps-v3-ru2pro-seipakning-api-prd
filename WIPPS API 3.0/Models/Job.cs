using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class Job
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public long RefineryId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Refinery Refinery { get; set; }
    }
}
