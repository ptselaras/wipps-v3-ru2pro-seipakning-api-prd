using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models
{
    public class FormType3
    {
        public long id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string file { get; set; }
        public long? refinery_id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public long? ord { get; set; }
        public int? checklists_count { get; set; }
        public int? checklists_pos_count { get; set; }
        public int? checklists_neg_count { get; set; }
    }
}
