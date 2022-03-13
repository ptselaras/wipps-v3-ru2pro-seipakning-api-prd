using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Utils
{
    public static class Converter
    {
        public static string mimeToText(string mime)
        {
            string json = @"{
                               'png':['image/png', 'image/x-png'],
                               'bmp':['image/bmp', 'image/x-bmp', 'image/x-bitmap', 'image/x-xbitmap'],
                               'jpeg':['image/jpeg', 'image/pjpeg']
                          }";

            MimeType detail = JsonConvert.DeserializeObject<MimeType>(json);

            foreach (PropertyInfo property in detail.GetType().GetProperties())
            {
                string Key = property.Name;
                List<string> Value = (List<string>) property.GetValue(detail, null);

                if (Value.Any(x => x.Contains(mime))) return Key;
            }

            return "";

        }

        public static string base64ToMime(string data)
        {
            string text = data.Substring(0, 1);
            if (text == "/")
            {
                return "image/jpeg";
            }else if (text == "i")
            {
                return "image/png";
            }
            else
            {
                return "image/png";
            }

            return "";
        }

        public class MimeType
        {
            public List<string> png { get; set; }
            public List<string> bmp { get; set; }
            public List<string> jpeg { get; set; }
        }
    }
}
