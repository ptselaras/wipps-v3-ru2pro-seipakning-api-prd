using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Procedure
{
    public class GetInspection
    {
        public int inspection { get; set; }
        public int pos { get; set; }
        public int neg { get; set; }
        public decimal? safe { get; set; }
        public decimal? @unsafe { get; set; }
        public decimal? percentage { get; set; }
        public string month { get; set; }
        public string month_n { get; set; }
    }
}
