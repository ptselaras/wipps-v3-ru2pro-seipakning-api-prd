using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Controllers.Response
{
    public static class Auth
    {
        public class ResponseUser
        {
            public object user { get; set; }
            public string token { get; set; }
            public bool success { get; set; }
        }
    }
}
