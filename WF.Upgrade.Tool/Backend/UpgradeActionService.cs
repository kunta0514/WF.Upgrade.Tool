using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using WF.DbProvider;
using WF.Upgrade.Model;
using WF.Upgrade.Model.CustomAttribute;
using WF.Upgrade.Public;

namespace WF.Upgrade.Tool.Backend
{
    public class UpgradeActionService
    {

        public UpgradeActionService()
        {
            new ConnectionScope(InitDb.ConnectionStr());
        }

        private static List<RuleInfo> RuleInfoList { get; set; }

        private static List<Type> CheckRuleTypeList = new List<Type>();

        public string GetUpgradeActionList()
        {
            if (RuleInfoList == null)
            {
                var assembyle = Assembly.Load("WF.UpgradeAction.Business");

                var typeList = assembyle.GetTypes();

                var ruleInfoList = new List<RuleInfo>();

                foreach (Type typeAction in typeList.Where(typeRule => typeof(IUpgradeAction).IsAssignableFrom(typeRule)))
                {
                    CheckRuleTypeList.Add(typeAction);
                    var list = typeAction.GetCustomAttributes(true);
                    var info = new RuleInfo();
                    foreach (var att in list)
                    {
                        var attType = att.GetType();

                        switch (attType.Name)
                        {
                            case "FromVersionAttribute":
                                info.FromVersion = ((FromVersionAttribute)att).FromVersion;
                                break;
                            case "ToVersionAttribute":
                                info.FromVersion = ((ToVersionAttribute)att).ToVersion;
                                break;
                            case "CheckRuleTypeAttribute":
                                info.CheckRuleType = ((CheckRuleTypeAttribute)att).CheckRuleType;
                                break;
                            case "CheckRuleRemarkAttribute":
                                info.CheckRuleRemark = ((CheckRuleRemarkAttribute)att).CheckRuleRemark;
                                break;
                            case "CheckRuleKindAttribute":
                                info.CheckRuleKind = ((CheckRuleKindAttribute)att).CheckRuleKind;
                                break;
                            case "CheckRuleNameAttribute":
                                info.CheckRuleName = ((CheckRuleNameAttribute)att).CheckRuleName;
                                break;
                        }
                    }

                    info.IsCheck = false;
                    ruleInfoList.Add(info);
                }
                RuleInfoList = ruleInfoList;
            }
            return JsonConvert.SerializeObject(RuleInfoList);
        }

        public string ActionRule(string ruleName)
        {
            var type = CheckRuleTypeList.Find(t => t.Name == ruleName);

            var result = new UpgradeActionResult();

            var ruleInfo = RuleInfoList.Find(t => t.CheckRuleName == ruleName);
            ruleInfo.CheckBegin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (type != null)
            {
                var rule = Activator.CreateInstance(type) as IUpgradeAction;
                result = rule.Action(null);

                //结果记录
                ruleInfo.CheckEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ruleInfo.IsCheck = true;
                ruleInfo.ActionResult = result;

            }
            return JsonConvert.SerializeObject(ruleInfo);
        }

      
    }
}
