using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class FormType
    {
        public FormType()
        {
            FormComponents = new HashSet<FormComponent>();
            Forms = new HashSet<Form>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string File { get; set; }
        public long RefineryId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? Ord { get; set; }

        public virtual Refinery Refinery { get; set; }
        public virtual ICollection<FormComponent> FormComponents { get; set; }
        public virtual ICollection<Form> Forms { get; set; }
    }
}
