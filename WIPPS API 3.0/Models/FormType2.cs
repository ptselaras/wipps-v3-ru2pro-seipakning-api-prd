using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models
{
    public class FormType2
    {
        public long id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string file { get; set; }
        public long? refinery_id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public long? ord { get; set; }
        public string elipsis { get; set; }
        public decimal? checklists_count { get; set; }
        public decimal? checklists_pos_count { get; set; }
        public decimal? checklists_neg_count { get; set; }
        public decimal? pos { get; set; }
        public decimal? neg { get; set; }
        public decimal? percentage { get; set; }
        public decimal? percentagen { get; set; }
        public decimal? total { get; set; }
    }
}
