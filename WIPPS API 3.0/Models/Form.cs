using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class Form
    {
        public Form()
        {
            ComponentChecklists = new HashSet<ComponentChecklist>();
        }

        public long Id { get; set; }
        public long AreaId { get; set; }
        public long JobId { get; set; }
        public long CompanyId { get; set; }
        public long FormTypeId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public long CreatedUserId { get; set; }
        public long UpdatedUserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Equipment { get; set; }

        public virtual Area Area { get; set; }
        public virtual Company Company { get; set; }
        public virtual FormType FormType { get; set; }
        public virtual ICollection<ComponentChecklist> ComponentChecklists { get; set; }
    }
}
