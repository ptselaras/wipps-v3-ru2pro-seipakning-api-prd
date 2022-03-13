using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static WIPPS_API_3._0.Controllers.Request;

namespace WIPPS_API_3._0.Services
{
    public interface IUriService
    {
        public Uri GetPageUri(PaginationFilter filter, string route);
    }
}
