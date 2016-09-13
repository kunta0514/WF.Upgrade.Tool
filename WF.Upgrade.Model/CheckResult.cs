using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Model
{
    public class CheckResult
    {

        public string CheckRuleName { set; get; }

        public object RepairParamList { get; set; }

        public string ErrorTitle { get; set; }

        public string ErrorCode { get; set; }

        public List<string> ErrorList { get; set; }

        public string ExceptionMessage { get; set; }
    }
}
