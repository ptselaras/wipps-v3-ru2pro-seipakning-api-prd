using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Chart
{
    public class GetDetailType
    {
        public string type { get; set; }
        public string slug { get; set; }
        public string name { get; set; }
        public long status { get; set; }
        public int? total { get; set; }
        public long form_component_id { get; set; }
    }
}
