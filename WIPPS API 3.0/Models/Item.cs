using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class Item
    {
        public Item()
        {
            ItemInspections = new HashSet<ItemInspection>();
            ItemRequirements = new HashSet<ItemRequirement>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string File { get; set; }
        public long RefineryId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Refinery Refinery { get; set; }
        public virtual ICollection<ItemInspection> ItemInspections { get; set; }
        public virtual ICollection<ItemRequirement> ItemRequirements { get; set; }
    }
}
