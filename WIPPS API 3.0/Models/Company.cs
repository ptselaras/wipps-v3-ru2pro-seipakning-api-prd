using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WIPPS_API_3._0.Models
{
    public partial class Company
    {
        public Company()
        {
            Forms = new HashSet<Form>();
            ItemInspections = new HashSet<ItemInspection>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public string Slug { get; set; }
        public string Type { get; set; }
        public long RefineryId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Refinery Refinery { get; set; }
        public virtual ICollection<Form> Forms { get; set; }
        public virtual ICollection<ItemInspection> ItemInspections { get; set; }
    }
}
