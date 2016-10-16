using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Tool.Model
{
    public class SiteInfo
    {
        public string CustomCode { get; set; }
        public string CustomName { get; set; }
        public string ERPUrl { get; set; }
        public string WFUrl { get; set; }
        public string ERPVersion { get; set; }
        public string DBServerName { get; set; }
        public string DBName { get; set; }
        public string DBUserName { get; set; }
        public string DBSaPassword { get; set; }
        public string UpFilesAddress { get; set; }

        public string WFAddress { get; set; }
    }
}
