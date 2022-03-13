using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Utils
{
    public static class Attributes
    {
        public static string getElipsisAttribue(string name)
        {
            if(name.Length > 16)
            {
                return name.Substring(0, 15) + "..";
            }
            return name;
        }

        public static string getPosAttribute(int? checklists_pos_count)
        {
            return checklists_pos_count.ToString() ?? "0";
        }

        public static string getNegAttribute(int? checklists_neg_count)
        {
            return checklists_neg_count.ToString() ?? "0";
        }

        public static string getTotalAttribute(int? checklists_count, int? checklists_pos_count)
        {
            return (checklists_count ?? 0) == 0 ? "0.00" : (Math.Ceiling((double)(((checklists_pos_count ?? 0.0) / (checklists_count ?? 0.0)) * 100.0) * 100.0) / 100.0).ToString();
        }

        public static string getPercentageAttribute(int? checklists_count, int? checklists_pos_count)
        {
            return (checklists_count ?? 0) == 0 ? "0.00" : (Math.Ceiling((double)(((checklists_pos_count ?? 0.0) / (checklists_count ?? 0.0)) * 100.0) * 100.0) / 100.0).ToString();
        }

        public static string getPercentagenAttribute(int? checklists_count, int? checklists_neg_count)
        {
            return (checklists_count ?? 0) == 0 ? "0.00" : (Math.Ceiling((double)(((checklists_neg_count ?? 0.0) / (checklists_count ?? 0.0)) * 100.0) * 100.0) / 100.0).ToString();
        }

        public static string getQueryTable(string[] form, string table, string search, long? refinery_id, IEnumerable<Dictionary<string, string>> order)
        {
            var columnsSearch = form;

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT * FROM "+ table + " ");

            if(refinery_id != null)
            {
                SqlStr.Append(" WHERE ");
            }

            if (search != null)
            {
                var index = 0;
                foreach (var column in columnsSearch)
                {
                    if ((columnsSearch.Length - 1) == index)
                    {
                        SqlStr.Append(column + " like '%" + search + "%' ");
                    }
                    else
                    {
                        SqlStr.Append(column + " like '%" + search + "%' OR ");
                    }

                    index++;
                }

                if(refinery_id != null)
                {
                    SqlStr.Append(" AND refinery_id = " + refinery_id + "");
                }
            }
            else
            {
                if(refinery_id != null)
                {
                    SqlStr.Append(" refinery_id = " + refinery_id + "");
                }
                
            }

            foreach (var ord in order.ToList())
            {
                if (ord["column"] == "0")
                {
                    SqlStr.Append(" ORDER BY created_at " + ord["dir"] + " ");
                }
                else
                {
                    SqlStr.Append(" ORDER BY " + columnsSearch[int.Parse(ord["column"])] + " " + ord["dir"] + " ");
                }
            }

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        public static string ImageToBase64(string path)
        {
            return Convert.ToBase64String(File.ReadAllBytes(path));
        }
    }
}
