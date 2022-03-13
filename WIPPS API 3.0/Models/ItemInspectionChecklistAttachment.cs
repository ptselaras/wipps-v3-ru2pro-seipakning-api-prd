using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class ItemInspectionChecklistAttachment
    {
        public long Id { get; set; }
        public long ItemInspectionChecklistId { get; set; }
        public string File { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ItemInspectionChecklist ItemInspectionChecklist { get; set; }
    }
}
