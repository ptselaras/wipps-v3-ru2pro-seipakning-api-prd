using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Procedure
{
    public class GetCompany
    {
        public string companies { get; set; }
        public string company { get; set; }
        public string slug { get; set; }
        public int pos { get; set; }
        public int neg { get; set; }
        public decimal? safe { get; set; }
        public decimal? @unsafe { get; set; }
        public decimal? total { get; set; }
    }
}
