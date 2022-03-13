using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models
{
    public class FormPercentage
    {
        public long Id { get; set; }
        public long FormId { get; set; }
        public long FormComponentId { get; set; }
        public long Status { get; set; }
        public int? SafeValue { get; set; }
        public int? UnsafeValue { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Inspection { get; set; }
        public string Safe { get; set; }
        public string Unsafe { get; set; }
        public string Percentage { get; set; }
    }
}
