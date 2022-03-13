using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Chart
{
    public class GetCompany
    {
        public string companies { get; set; }
        public string company { get; set; }
        public int pos { get; set; }
        public int neg { get; set; }
        public int? safe { get; set; }
        public int? @unsafe { get; set; }
        public decimal? total { get; set; }
    }
}
