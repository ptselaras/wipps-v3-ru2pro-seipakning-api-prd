using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class ItemInspectionChecklist
    {
        public ItemInspectionChecklist()
        {
            ItemInspectionChecklistAttachments = new HashSet<ItemInspectionChecklistAttachment>();
        }

        public long Id { get; set; }
        public long ItemInspectionId { get; set; }
        public long ItemRequirementId { get; set; }
        public long Status { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ItemInspection ItemInspection { get; set; }
        public virtual ItemRequirement ItemRequirement { get; set; }
        public virtual ICollection<ItemInspectionChecklistAttachment> ItemInspectionChecklistAttachments { get; set; }
    }
}
