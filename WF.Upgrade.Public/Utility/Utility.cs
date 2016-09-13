using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Public
{
    public class Utility
    {
        public static List<Dictionary<string, string>> ToListDic(DataTable table)
        {
            var list = new List<Dictionary<string, string>>();
            foreach (DataRow row in table.Rows)
            {
                var dic = new Dictionary<string, string>();
                foreach (DataColumn col in table.Columns)
                {
                    var colName = col.ColumnName;
                    var colValue = row[colName].ToString();
                    dic.Add(colName, colValue);
                }
                list.Add(dic);
            }
            return list;
        }

        public static Dictionary<string, string> ToDic(DataTable table)
        {
            if (table.Rows.Count == 0) return null;
            var dic = new Dictionary<string, string>();
            foreach (DataColumn col in table.Columns)
            {
                var colName = col.ColumnName;
                var colValue = table.Rows[0][colName].ToString();
                dic.Add(colName, colValue);
            }
            return dic;
        }

        public static string GetBaseDirectory()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            path = path.Replace(@"\bin\", @"\").Replace(@"\bin", @"\");

            return path;
        }
    }
}
