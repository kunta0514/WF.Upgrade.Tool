using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Model
{
    public class RuleInfo
    {
        public List<string> FromVersion { get; set; }

        public List<string> ToVersion { get; set; }

        public string CheckRuleRemark { get; set; }

        public string CheckRuleType { get; set; }

        public string CheckRuleKind { get; set; }

        public string CheckRuleName { get; set; }

        public bool IsCheck { get; set; }

        public string CheckBegin { get; set; }

        public string CheckEnd { get; set; }

        public CheckResult CheckResult { get; set; }

        public UpgradeActionResult ActionResult { get; set; }
    }
}
