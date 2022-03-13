using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WIPPS_API_3._0.Models
{
    public partial class Area
    {

        public Area()
        {
            AreaEmails = new HashSet<AreaEmail>();
            Forms = new HashSet<Form>();
            ItemInspections = new HashSet<ItemInspection>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string Slug { get; set; }
        public long RefineryId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Refinery Refinery { get; set; }
        public virtual ICollection<AreaEmail> AreaEmails { get; set; }
        public virtual ICollection<Form> Forms { get; set; }
        public virtual ICollection<ItemInspection> ItemInspections { get; set; }
    }
}
