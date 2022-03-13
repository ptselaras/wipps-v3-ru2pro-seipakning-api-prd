using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Controllers
{
    public static class Request
    {
        public class PaginationFilter
        {
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public PaginationFilter()
            {
                this.PageNumber = 1;
                this.PageSize = 10;
            }

            public PaginationFilter(int pageNumber, int pageSize)
            {
                this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
                this.PageSize = pageSize < 10 ? 10 : pageSize;
            }
        }
    }
}
