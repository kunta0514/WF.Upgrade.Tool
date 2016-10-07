using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using WF.Upgrade.Framework;

namespace WF.Upgrade.Business
{
    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("before-up")]
    [CheckRuleRemark("已启用的流程模版中没有所属系统，请检查是否为通用审批流，可能会引起流程无法发起")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("已启用的流程模版中没有所属系统")]
    public class Module_Application : ICheckRule
    {
        public CheckResult Check(object input)
        {
            //TODO:自动生成修复语，合并到升级语句中，执行顺序为1（升级前处理）
            //TODO:模板3要素+被删除的模板，但是实例中存在的
            string sql = @"select * from myWorkflowProcessModule where ISNULL(application,'') = '' and IsActive = 1";
            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "已启用的流程模版中没有所属系统：" + row["ProcessName"]).ToList();
            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"TableName", "myWorkflowProcessModule"},
                                                {"ProcessName", row["ProcessName"].ToString()},
                                                {"ProcessGUID",row["ProcessGUID"].ToString()}
                                            }).ToList();
            //Repair(null);
            return new CheckResult
            {
                ErrorList = errorList,
                RepairParamList = repairParamList,
                ErrorCode = "10101"
            };
        }

        public object Repair(object input)
        {
            //TODO:修复方案待确认，目前看是有问题的
            string temp = @"INSERT INTO myworkflowbiztype (BusinessTypeName,Application,description) VALUES('{0}','{1}','{2}');";
            string strRepair = @"UPDATE a SET a.application=b.application from myWorkflowProcessModule a LEFT JOIN myWorkflowBizType
                        b ON b.BusinessTypeName = a.BusinessType where ISNULL(a.application,'') = '' and IsActive = 1 AND b.Application<>''";
            //256等低版本业务类型都配置在workflow.confi文件中 
            //TODO:调整成直接通过erp站点来读取
            string _wfconfigPath = Utility.GetBaseDirectory() + "Config/Worklfow.config";
            if (!File.Exists(_wfconfigPath))
            {
                return null;
            }
            XmlDocument xmlDoc = new XmlDocument();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                xmlDoc.Load(_wfconfigPath);
                XmlNodeList xmlList = xmlDoc.SelectNodes("//item");
                foreach (XmlNode item in xmlList)
                {
                    dic.Add(item.ParentNode.Attributes["id"].Value.ToString() + "-" + item.Attributes["name"].Value.ToString(), item.ParentNode.Attributes["id"].Value.ToString());
                }
            }
            catch (Exception ex)
            {

                throw;
            }


            StringBuilder sb = new StringBuilder();

            foreach (var item in dic)
            {
                sb.Append(string.Format(temp,item.Key.Substring(5, item.Key.Length-5), item.Value,"Workflow.config"));
            }
            return string.Concat(sb.ToString(),strRepair);
        }
    }
}
