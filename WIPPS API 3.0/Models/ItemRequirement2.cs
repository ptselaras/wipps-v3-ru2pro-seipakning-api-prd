using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models
{
    public partial class ItemRequirement2
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public long RequirementId { get; set; }
        public long? Order { get; set; }
        public string ItemName { get; set; }
        public string RequirementSlug { get; set; }
        public string RequirementName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
