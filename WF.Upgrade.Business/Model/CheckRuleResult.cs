using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Upgrade.Business
{
    public class CheckRuleResult
    {
        public int rule_id { get; set; }
        public string rule_name { get; set; }
        public string ex_msg { get; set; }
        public string repair_result { get; set; }
        public string repair_param { get; set; }
    }
}
