using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models
{
    public partial class ItemInspection2
    {
        public long Id { get; set; }
        public string AreaName { get; set; }
        public long AreaId { get; set; }
        public long ItemId { get; set; }
        public string CompanyName { get; set; }
        public long CompanyId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Slug { get; set; }
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
    }
}
