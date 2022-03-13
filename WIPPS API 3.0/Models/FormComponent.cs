using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class FormComponent
    {
        public FormComponent()
        {
            ComponentChecklists = new HashSet<ComponentChecklist>();
        }

        public long Id { get; set; }
        public long FormTypeId { get; set; }
        public long ComponentId { get; set; }
        public long? Order { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Component Component { get; set; }
        public virtual FormType FormType { get; set; }
        public virtual ICollection<ComponentChecklist> ComponentChecklists { get; set; }
    }
}
