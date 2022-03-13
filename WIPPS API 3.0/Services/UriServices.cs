using Microsoft.AspNetCore.WebUtilities;
using WIPPS_API_3._0.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Services
{
    public class UriServices : IUriService
    {
        private readonly string _baseUri;

        public UriServices(string baseUri)
        {
            _baseUri = baseUri;
        }

        public Uri GetPageUri(Request.PaginationFilter filter, string route)
        {
            var _endpointUri = new Uri(string.Concat(_baseUri, route));
            var modifiedUri = QueryHelpers.AddQueryString(_endpointUri.ToString(), "page", filter.PageNumber.ToString());
            return new Uri(modifiedUri);
        }
    }
}
