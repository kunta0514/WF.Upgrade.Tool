using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using WF.Upgrade.Tool.Model;
using WF.Upgrade.Business;
using System.Windows;
using System.Data;

namespace WF.Upgrade.Tool.Backend
{
    public class CheckRuleService
    {
        public CheckRuleService()
        {
            //new ConnectionScope(InitDb.ConnectionStr());
        }

        private static List<Business.CheckRule> checkRuleList { get; set; }

        private static Dictionary<string, Type> checkRuleTypeList = new Dictionary<string, Type>(); 

        private Dictionary<string,int>  selectedRuleKind = new Dictionary<string, int>();

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
                if (checkRuleList == null)
                {
                    var assembyle = Assembly.Load("WF.Upgrade.Business");
                    var typeList = assembyle.GetTypes();
                    var ruleInfoList = new List<Business.CheckRule>();

                    foreach (Type typeRule in typeList.Where(typeRule => typeof (ICheckRule).IsAssignableFrom(typeRule))  )
                    {

                        var attrlist = typeRule.GetCustomAttributes(true);
                        var info = new Business.CheckRule();
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
                                    info.type = ((CheckRuleTypeAttribute) att).CheckRuleType;
                                    break;
                                case "CheckRuleRemarkAttribute":
                                    info.remark = ((CheckRuleRemarkAttribute) att).CheckRuleRemark;
                                    break;
                                case "CheckRuleKindAttribute":
                                    info.kind = ((CheckRuleKindAttribute) att).CheckRuleKind;
                                    break;
                                case "CheckRuleNameAttribute":
                                    info.name = ((CheckRuleNameAttribute) att).CheckRuleName;
                                    break;
                            }
                        }
                        if (string.IsNullOrEmpty(info.name))
                        {
                            continue;
                        }
                        info.is_check = 0;
                        ruleInfoList.Add(info);
                        checkRuleTypeList.Add(info.name, typeRule);
                                           
                    }
                    checkRuleList = ruleInfoList;
                    sync_check_rule(checkRuleList);
                }              
                var result = new
                             {
                                 result = 1,
                                 message = string.Empty,
                                 data = checkRuleList,
                                 checkRuleKindList = checkRuleList.Select(info => info.kind).Distinct().ToList()
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
            List<Business.CheckRule> resultList = new List<Business.CheckRule>();
            if (dic.ContainsKey("CheckRuleKind") && !string.IsNullOrEmpty(dic["CheckRuleKind"]))
            {
                resultList = checkRuleList.Where(info => info.kind == dic["CheckRuleKind"]).ToList();
            }

            return JsonConvert.SerializeObject(resultList);
            
        }

        public string checkRule(string ruleName)
        {
            try
            {
                var ruleInfo = Check(ruleName);
                //TODO::需要按照新格式返回
                return JsonConvert.SerializeObject(ruleInfo);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { err_code = -1, message = ex.Message });
            }
        }

        private CheckRule Check(string ruleName)
        {
            //TODO::此处改造成本地库读取方式
            var type = checkRuleTypeList.Where(kv => kv.Key == ruleName).First().Value;
            var checkRuleInfo = checkRuleList.Find(t => t.name == ruleName);

            //TODO::此处统一了各地方CheckRule的model，可以直接用反射后的返回值复制。
            checkRuleInfo.begin_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (type != null)
            {
                CheckRule result;
                try
                {
                    var rule = Activator.CreateInstance(type) as ICheckRule;
                    result = rule.Check(null);
                }
                catch (Exception ex)
                {
                    //return null;
                    result = new CheckRule
                    {
                        err_msg = "运行规则失败！",
                        err_code = "-1"
                    };
                }
                //结果记录
                checkRuleInfo.is_check = 1;
                checkRuleInfo.ex_list = result.ex_list;
            }
            checkRuleInfo.end_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            sync_check_rule(checkRuleInfo);
            return checkRuleInfo;
        }

        private void sync_check_rule(CheckRule rule)
        {
            //ruleinfo.CheckResult.ex_list;
            string sql = string.Format(@"update p_check_rule set begin_time={1},end_time={2},result_count={3},is_check={4},check_count={5},err_code={6},err_msg={7} where name={0}",
                rule.name, rule.begin_time, rule.end_time, 55, 1, rule.check_count, rule.err_code, rule.err_msg);
        }

        private void sync_check_rule(List<Business.CheckRule> ruleInfoList)
        {
            foreach (var item in ruleInfoList)
            {
                //检查规则是否已经存在
                string sql = string.Format(@"select 1 from p_check_rule where name = '{0}'", item.name);
                if (SQLiteHelper.ExecuteScalar(System.Data.CommandType.Text, sql) != 1)
                {
                    sql = string.Format(@"insert into p_check_rule(name,kind,type,remark) values ('{0}','{1}','{2}','{3}')", item.name, item.kind, item.type, item.remark);
                    SQLiteHelper.ExecuteNonQuery(System.Data.CommandType.Text, sql);
                }
            }
        }
        
        public string viewRule(string ruleName)
        {
            var info = checkRuleList.Find(r => r.name == ruleName);

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
