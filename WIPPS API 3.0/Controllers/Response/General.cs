using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Controllers.Response
{
    public static class General
    {
        public class ResponseFirst
        {
            public int code { get; set; }
            public object data { get; set; }
            public bool success { get; set; }
        }

        public class ResponseSecond
        {
            public int code { get; set; }
            public object data { get; set; }
        }

        public class ResponseThird
        {
            public int total { get; set; }
            public List<object> data { get; set; }
        }

        public class ResponseFourth
        {
            public int code { get; set; }
            public List<object> data { get; set; }
        }

        public class ResponseFifth
        {
            public int code { get; set; }
            public string title { get; set; }
            public string content { get; set; }
            public string data { get; set; }
            public bool success { get; set; }
        }

        public class ResponseError
        {
            public object errors { get; set; }
            public string message { get; set; }
        }
    }
}
