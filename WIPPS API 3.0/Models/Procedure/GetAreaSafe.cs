using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Procedure
{
    public class GetAreaSafe
    {
        public string name { get; set; }
        public string slug { get; set; }
        public decimal? pos { get; set; }
        public decimal? neg { get; set; }
        public decimal? total { get; set; }
    }
}
