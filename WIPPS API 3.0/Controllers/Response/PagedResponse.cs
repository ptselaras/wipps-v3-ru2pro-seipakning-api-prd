using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Controllers.Response
{
    public class PagedResponse<T> : Response<T>
    {
        public int current_page { get; set; }
        public int from { get; set; }
        public int to { get; set; }
        public string per_page { get; set; }
        public Uri first_page_url { get; set; }
        public Uri last_page_url { get; set; }
        public int last_page { get; set; }
        public int total { get; set; }
        public Uri next_page_url { get; set; }
        public Uri prev_page_url { get; set; }

        public PagedResponse(object data, int pageNumber, int pageSize)
        {
            this.from = 1;
            this.current_page = pageNumber;
            this.last_page = pageSize;
            this.per_page = pageSize.ToString();
            this.to = pageSize;
            this.Data = data;
        }
    }
}
