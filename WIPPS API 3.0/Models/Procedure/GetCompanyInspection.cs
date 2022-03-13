using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Procedure
{
    public class GetCompanyInspection
    {
        public long id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string type { get; set; }
        public long? refinery_id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string companies { get; set; }
        public int forms_count { get; set; }
        public int total { get; set; }
    }
}
