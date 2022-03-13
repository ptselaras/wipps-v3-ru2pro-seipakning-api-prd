using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models
{
    public class ComponentChecklist2
    {
        public long Id { get; set; }
        public long FormId { get; set; }
        public long FormComponentId { get; set; }
        public long Status { get; set; }
        public int? SafeValue { get; set; }
        public int? UnsafeValue { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Type { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public long? Total { get; set; }
    }
}
