using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Business
{
    public class FromVersionAttribute : Attribute 
    {
        public FromVersionAttribute(string fromVersion)
        {
            this.FromVersion = fromVersion.Split(',').ToList();
        }

        public List<string> FromVersion { get; set; }
    }

    public class ToVersionAttribute : Attribute
    {
        public ToVersionAttribute(string toVersion)
        {
            this.ToVersion = toVersion.Split(',').ToList();
        }

        public List<string> ToVersion { get; set; }

    }

    public class CheckRuleTypeAttribute : Attribute
    {
        public CheckRuleTypeAttribute(string checkRuleType)
        {

            this.CheckRuleType = checkRuleType;
          

        }

        public string CheckRuleType { get; set; }

    }

    public class CheckRuleRemarkAttribute : Attribute
    {
        public CheckRuleRemarkAttribute(string checkRuleRemark)
        {

            this.CheckRuleRemark = checkRuleRemark;

        }
        public string CheckRuleRemark { get; set; }
    }

    public class CheckRuleKindAttribute : Attribute
    {
        public CheckRuleKindAttribute(string checkRuleKind)
        {

            this.CheckRuleKind = checkRuleKind;

        }
        public string CheckRuleKind { get; set; }
    }

    public class CheckRuleNameAttribute : Attribute
    {
        public CheckRuleNameAttribute(string checkRuleName)
        {

            this.CheckRuleName = checkRuleName;

        }
        public string CheckRuleName { get; set; }
    }
}
