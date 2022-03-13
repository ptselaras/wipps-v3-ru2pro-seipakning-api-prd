using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class ItemRequirement
    {
        public ItemRequirement()
        {
            ItemInspectionChecklists = new HashSet<ItemInspectionChecklist>();
        }

        public long Id { get; set; }
        public long ItemId { get; set; }
        public long RequirementId { get; set; }
        public long? Order { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Item Item { get; set; }
        public virtual Requirement Requirement { get; set; }
        public virtual ICollection<ItemInspectionChecklist> ItemInspectionChecklists { get; set; }
    }
}
