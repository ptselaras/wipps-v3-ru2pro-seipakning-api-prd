using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models.Chart
{
    public class GetInspection
    {
        public int? inspection { get; set; }
        public int? safe { get; set; }
        public int? @unsafe { get; set; }
        public string month { get; set; }
        public DateTime? created_at { get; set; }
    }
}
