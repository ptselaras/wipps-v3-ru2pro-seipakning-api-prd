using WIPPS_API_3._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Utils
{
    public static class DataStorage
    {
        public static List<Form2> GetAllForms() =>
            new List<Form2>
            {
                new Form2 {AreaName = "Areas 1", CompanyName = "Company 1", FormName = "Form 1"},
                new Form2 {AreaName = "Areas 2", CompanyName = "Company 2", FormName = "Form 2"},
                new Form2 {AreaName = "Areas 3", CompanyName = "Company 3", FormName = "Form 3"},
                new Form2 {AreaName = "Areas 4", CompanyName = "Company 4", FormName = "Form 4"},
                new Form2 {AreaName = "Areas 5", CompanyName = "Company 5", FormName = "Form 5"}
            };
    }
}
