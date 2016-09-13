using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WF.DbProvider;
using WF.Upgrade.Model;
using WF.Upgrade.Model.CustomAttribute;
using WF.Upgrade.Public;

namespace WF.CheckRule.Business.Form
{
    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("after-up")]
    [CheckRuleRemark("流程实例没有对应业务类型，请修复数据，会引起分类中出现其他选项")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("流程实例没有对应业务类型")]
    public class ProcessEntity : ICheckRule
    {
        public CheckResult Check(object input)
        {
            string sql = @"select ProcessGUID,a.ProcessName,a.BusinessTypeGUID , a.BusinessType,b.BusinessTypeGUID,b.BusinessTypeName 
                        FROM dbo.myWorkflowProcessEntity a LEFT JOIN dbo.myWorkflowBizType b ON b.BusinessTypeGUID = a.BusinessTypeGUID
                        WHERE b.BusinessTypeName IS  NULL";

            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "流程实例没有对应业务类型：" + row["ProcessName"]).ToList();
            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"TableName", "myWorkflowProcessEntity"},
                                                {"ProcessGUID", row["ProcessGUID"].ToString()},
                                                {"ProcessName", row["ProcessName"].ToString()}
                                            }).ToList();

            return new CheckResult
            {
                ErrorList = errorList,
                RepairParamList = repairParamList,
                ErrorCode = "10201"
            };
        }

        public object Repair(object input)
        {
            string strRepair = @"UPDATE a SET a.BusinessTypeGUID=b.BusinessTypeGUID FROM myWorkflowProcessEntity a LEFT JOIN myworkflowbiztype b ON a.BusinessType=b.businesstypename
WHERE a.BusinessType<>'' AND a.BusinessTypeGUID IS NULL";
            throw new NotImplementedException();
        }
    }
}
