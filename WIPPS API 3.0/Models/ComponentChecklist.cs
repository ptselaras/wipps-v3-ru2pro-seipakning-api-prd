using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class ComponentChecklist
    {
        public ComponentChecklist()
        {
            ComponentAttachments = new HashSet<ComponentAttachment>();
        }

        public long Id { get; set; }
        public long FormId { get; set; }
        public long FormComponentId { get; set; }
        public long Status { get; set; }
        public int? SafeValue { get; set; }
        public int? UnsafeValue { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Form Form { get; set; }
        public virtual FormComponent FormComponent { get; set; }
        public virtual ICollection<ComponentAttachment> ComponentAttachments { get; set; }
    }
}
