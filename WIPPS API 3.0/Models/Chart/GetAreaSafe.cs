using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Chart
{
    public class GetAreaSafe
    {
        public string name { get; set; }
        public int? pos { get; set; }
        public int? neg { get; set; }
        public decimal? total { get; set; }
    }
}
