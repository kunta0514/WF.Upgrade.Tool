using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using WF.DbProvider;
using WF.Upgrade.Model;
using WF.Upgrade.Model.CustomAttribute;
using WF.Upgrade.Public;
using System.Windows;
using System.Data;

namespace WF.Upgrade.Tool.Backend
{
    public class CheckRuleService
    {
        //public CheckRuleService()
        //{
        //    new ConnectionScope(InitDb.ConnectionStr());
        //}

        private List<RuleInfo> RuleInfoList { get; set; }

        private Dictionary<string, Type> CheckRuleTypeList = new Dictionary<string, Type>(); 

        private Dictionary<string,int>  SelectedRuleKind = new Dictionary<string, int>();

        //public void showMessage(string msg)
        //{
        //    MessageBox.Show(msg);
        //}

        public string getCheckRuleList(string type)
        {
            string sql = string.Format(@"select * from p_check_rule where type = '{0}'", type);
            DataSet ds = SQLiteHelper.ExecuteDataset(sql);
            DataTable dt = ds.Tables[0];
            return JsonHelp.ToJson(dt);
        }

        public void InitCheckRule()
        {
            try
            {
                if (RuleInfoList == null)
                {
                    var assembyle = Assembly.Load("WF.CheckRule.Business");
                    var typeList = assembyle.GetTypes();
                    var ruleInfoList = new List<RuleInfo>();

                    foreach (Type typeRule in typeList.Where(typeRule => typeof (ICheckRule).IsAssignableFrom(typeRule))  )
                    {

                        var attrlist = typeRule.GetCustomAttributes(true);
                        var info = new RuleInfo();
                        foreach (var att in attrlist)
                        {
                            var attType = att.GetType();

                            switch (attType.Name)
                            {
                                case "FromVersionAttribute":
                                    info.FromVersion = ((FromVersionAttribute) att).FromVersion;
                                    break;
                                case "ToVersionAttribute":
                                    info.FromVersion = ((ToVersionAttribute) att).ToVersion;
                                    break;
                                case "CheckRuleTypeAttribute":
                                    info.CheckRuleType = ((CheckRuleTypeAttribute) att).CheckRuleType;
                                    break;
                                case "CheckRuleRemarkAttribute":
                                    info.CheckRuleRemark = ((CheckRuleRemarkAttribute) att).CheckRuleRemark;
                                    break;
                                case "CheckRuleKindAttribute":
                                    info.CheckRuleKind = ((CheckRuleKindAttribute) att).CheckRuleKind;
                                    break;
                                case "CheckRuleNameAttribute":
                                    info.CheckRuleName = ((CheckRuleNameAttribute) att).CheckRuleName;
                                    break;
                            }
                        }
                        if (string.IsNullOrEmpty(info.CheckRuleName))
                        {
                            continue;
                        }
                        info.IsCheck = false;
                        ruleInfoList.Add(info);
                        CheckRuleTypeList.Add(info.CheckRuleName, typeRule);
                                           
                    }
                    RuleInfoList = ruleInfoList;
                    sync_check_rule(RuleInfoList);
                }              
                var result = new
                             {
                                 result = 1,
                                 message = string.Empty,
                                 data = RuleInfoList,
                                 checkRuleKindList = RuleInfoList.Select(info => info.CheckRuleKind).Distinct().ToList()
                             };
                
                //return JsonConvert.SerializeObject(result);
            }
            catch(Exception ex)
            {
                throw ex;
                //return JsonConvert.SerializeObject(new
                //             {
                //                 result = 0,
                //                 message = ex.Message
                //             });
            }
        }        

        public string GetRuleInfoListSearch(string input)
        {
            var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(input);
            List<RuleInfo> resultList = new List<RuleInfo>();
            if (dic.ContainsKey("CheckRuleKind") && !string.IsNullOrEmpty(dic["CheckRuleKind"]))
            {
                resultList = RuleInfoList.Where(info => info.CheckRuleKind == dic["CheckRuleKind"]).ToList();
            }

            return JsonConvert.SerializeObject(resultList);
            
        }

        public string checkRule(string ruleName)
        {
            try
            {
                var ruleInfo = Check(ruleName);
                return JsonConvert.SerializeObject(new
                                                   {
                                                       result = 1,
                                                       data = ruleInfo
                                                   });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                                                   {
                                                       result = 0,
                                                       message = ex.Message
                                                   });
            }
        }

        private RuleInfo Check(string ruleName)
        {
            var type = CheckRuleTypeList.Where(kv => kv.Key == ruleName).First().Value;

            var ruleInfo = RuleInfoList.Find(t => t.CheckRuleName == ruleName);
            ruleInfo.CheckBegin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (type != null)
            {
                CheckResult result;
                try
                {
                    var rule = Activator.CreateInstance(type) as ICheckRule;
                    result = rule.Check(null);
                }
                catch (Exception ex)
                {
                    result = new CheckResult
                             {
                                 ErrorTitle = "运行规则失败！",
                                 ErrorList = new List<string> {ex.Message}
                             };
                }
                //结果记录

                ruleInfo.IsCheck = true;
                ruleInfo.CheckResult = result;
            }
            ruleInfo.CheckEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            sync_check_rule(ruleInfo);
            return ruleInfo;
        }

        private void sync_check_rule(RuleInfo ruleinfo)
        {
            string sql = string.Format(@"update p_check_rule set begin_time={0},end_time={1},result_count={2},is_check={3},check_count={4},err_code={5},err_msg={6} where name={7}", 
                ruleinfo.CheckRuleName);
        }

        private void sync_check_rule(List<RuleInfo> ruleInfoList)
        {
            foreach (var item in ruleInfoList)
            {
                //检查规则是否已经存在
                string sql = string.Format(@"select 1 from p_check_rule where name = '{0}'", item.CheckRuleName);
                if (SQLiteHelper.ExecuteScalar(System.Data.CommandType.Text, sql) != 1)
                {
                    sql = string.Format(@"insert into p_check_rule(name,kind,type,remark) values ('{0}','{1}','{2}','{3}')", item.CheckRuleName, item.CheckRuleKind, item.CheckRuleType, item.CheckRuleRemark);
                    SQLiteHelper.ExecuteNonQuery(System.Data.CommandType.Text, sql);
                }
            }
        }
        
        public string ViewRule(string ruleName)
        {
            var info = RuleInfoList.Find(r => r.CheckRuleName == ruleName);

            return JsonConvert.SerializeObject(info);
        }

        public string CheckRules(string ruleNames)
        {
            var lisName = ruleNames.Split('|').ToList();

            var ruleInfoList = lisName.Select(name => Check(name)).ToList();

            return JsonConvert.SerializeObject(ruleInfoList);
        }

    }

    
}
