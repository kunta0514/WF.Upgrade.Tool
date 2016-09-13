using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WF.DbProvider;
using WF.Upgrade.Model;
using WF.Upgrade.Model.CustomAttribute;
using WF.Upgrade.Public;

namespace WF.CheckRule.Business.BusinessObject
{
    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("after-up")]
    [CheckRuleRemark("已启用的流程模版中没有业务对象、业务类型，请检查是否为通用审批流，可能会引起流程无法发起")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("已启用的流程模版中没有业务对象")]
    public class Module_BusinessObject : ICheckRule
    {
        public CheckResult Check(object input)
        {
            string sql = @"select * from myWorkflowProcessModule where BusinessTypeGUID is NULL and IsActive = 1";
            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "已启用的流程模版中没有业务对象：" + row["ProcessName"]).ToList();
            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"TableName", "myWorkflowProcessModule"},
                                                {"ProcessName", row["ProcessName"].ToString()}
                                            }).ToList();
            return new CheckResult
            {
                ErrorList = errorList,
                RepairParamList = repairParamList,
                ErrorCode = "10102"
            };
        }

        public object Repair(object input)
        {
            string strRepair =
                @"UPDATE a SET a.BusinessTypeGUID=b.BusinessTypeGUID FROM dbo.myWorkflowProcessModule a LEFT JOIN dbo.myworkflowbiztype b ON a.BusinessType=b.businesstypename
                WHERE a.BusinessType<>''";


            return null;
        }
    }
}
