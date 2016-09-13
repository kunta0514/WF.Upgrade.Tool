using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Model.DbUp
{
    public class DbUpSet
    {
        public string new_server { get; set; }
        public string new_username { get; set; }
        public string new_password { get; set; }
        public string new_dbname { get; set; }
        public string old_server { get; set; }
        public string old_username { get; set; }
        public string old_password { get; set; }
        public string old_dbname { get; set; }
        public decimal erp_version { get; set; }

        public string erp_source { get; set; }
    }
}
