using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models
{
    public class Form2
    {
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
        public string AreaName { get; set; }
        public string JobName { get; set; }
        public string CompanyName { get; set; }
        public string FormName { get; set; }
    }
}
