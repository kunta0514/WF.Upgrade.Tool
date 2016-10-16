using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Business
{

    public class CheckRule
    {
        public List<string> FromVersion { get; set; }

        public List<string> ToVersion { get; set; }

        public string name { get; set; }

        public string kind { get; set; }

        public string type { get; set; }

        public string remark { get; set; }

        public int is_check { get; set; }

        public string begin_time { get; set; }

        public string end_time { get; set; }

        public string err_code { get; set; }

        public string err_msg { get; set; }

        public int result_count { get; set; }

        public int check_count { get; set; }

        public object ex_list { get; set; }

        //public UpgradeActionResult ActionResult { get; set; }
    }
}
