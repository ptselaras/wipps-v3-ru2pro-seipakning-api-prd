using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Chart
{
    public class GetLocation
    {
        public long id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public long? refinery_id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int forms_count { get; set; }
    }
}
