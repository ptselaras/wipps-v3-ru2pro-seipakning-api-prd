using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Models
{
    public class FormComponent2
    {
        public long Id { get; set; }
        public long FormTypeId { get; set; }
        public long ComponentId { get; set; }
        public long? Order { get; set; }
        public string FormName { get; set; }
        public string ComponentSlug { get; set; }
        public string ComponentName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
