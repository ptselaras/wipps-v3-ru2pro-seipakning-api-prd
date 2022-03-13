using WIPPS_API_3._0.Controllers.Response;
using WIPPS_API_3._0.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static WIPPS_API_3._0.Controllers.Request;

namespace WIPPS_API_3._0.Helpers
{
    public static class PaginationHelper
    {
        public static PagedResponse<List<object>> CreatePagedResponse<T>(List<object> pagedData, PaginationFilter validFilter, int totalRecords, IUriService uriService, string route)
        {
            var response = new PagedResponse<List<object>>(pagedData, validFilter.PageNumber, validFilter.PageSize);
            var totalPages = ((double)totalRecords / (double)validFilter.PageSize);
            int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));
            response.next_page_url =
                validFilter.PageNumber >= 1 && validFilter.PageNumber < roundedTotalPages
                ? uriService.GetPageUri(new PaginationFilter(validFilter.PageNumber + 1, validFilter.PageSize), route)
                : null;
            response.prev_page_url =
                validFilter.PageNumber - 1 >= 1 && validFilter.PageNumber <= roundedTotalPages
                ? uriService.GetPageUri(new PaginationFilter(validFilter.PageNumber - 1, validFilter.PageSize), route)
                : null;
            response.first_page_url = uriService.GetPageUri(new PaginationFilter(1, validFilter.PageSize), route);
            response.last_page_url = uriService.GetPageUri(new PaginationFilter(roundedTotalPages, validFilter.PageSize), route);
            response.last_page = roundedTotalPages;
            response.total = totalRecords;

            return response;
        }
    }
}
