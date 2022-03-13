using System;
using System.Collections.Generic;

namespace WIPPS_API_3._0.Models
{
    public partial class Refinery
    {
        public Refinery()
        {
            Areas = new HashSet<Area>();
            Companies = new HashSet<Company>();
            Components = new HashSet<Component>();
            FormTypes = new HashSet<FormType>();
            Items = new HashSet<Item>();
            Jobs = new HashSet<Job>();
            Requirements = new HashSet<Requirement>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Area> Areas { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
        public virtual ICollection<Component> Components { get; set; }
        public virtual ICollection<FormType> FormTypes { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<Requirement> Requirements { get; set; }
    }
}
