using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Controllers.Response
{
    public class Response<T>
    {
        public object Data { get; set; }
        public Response()
        {

        }

        public Response(object data)
        {
            Data = data;
        }
    }
}
