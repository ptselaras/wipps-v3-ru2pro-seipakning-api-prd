using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WIPPS_API_3._0.Models
{
    public partial class ItemInspection
    {
        public ItemInspection()
        {
            ItemInspectionChecklists = new HashSet<ItemInspectionChecklist>();
        }

        public long Id { get; set; }
        [NotMapped]
        public string AreaName { get; set; }
        public long AreaId { get; set; }
        public long ItemId { get; set; }
        [NotMapped]
        public string CompanyName { get; set; }
        public long CompanyId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Slug { get; set; }
        [NotMapped]
        public string SlugItem { get; set; }
        public string Barcode { get; set; }
        public long RefineryId { get; set; }
        public long CreatedUserId { get; set; }
        public long UpdatedUserId { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Safetyman { get; set; }
        public string Inspector { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        
        public virtual Area Area { get; set; }
        public virtual Company Company { get; set; }
        public virtual Item Item { get; set; }
        public virtual ICollection<ItemInspectionChecklist> ItemInspectionChecklists { get; set; }
    }
}
