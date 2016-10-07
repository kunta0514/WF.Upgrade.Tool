using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WF.Upgrade.Framework;

namespace WF.Upgrade.Business
{
    //[FromVersion("305sp1,306,307,307sp1,307sp2")]
    //[ToVersion("306,307,307sp1,307sp2,308")]
    //[CheckRuleType("before-up")]
    //[CheckRuleRemark("在途流程中状态为[重新发起]的流程，需要调用校稿才能确保能够正常发起")]
    //[CheckRuleKind("数据检测")]
    //[CheckRuleName("在途流程中状态为[重新发起]的流程")]
    public class ProcessStepPathEntity : ICheckRule
    {
        public CheckResult Check(object input)
        {
            string sql = @"SELECT a.StepName,b.ProcessGUID,b.ProcessName FROM myWorkflowStepPathEntity a LEFT JOIN dbo.myWorkflowProcessEntity b 
                            ON b.ProcessGUID = a.ProcessGUID WHERE StepType =0 AND StepPathID > 1 AND StepStatus IN(1,2) AND b.ProcessGUID IS NOT NULL AND b.ProcessStatus in (0,1)";

            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "历史流程中状态为[重新发起]的流程：" + row["ProcessName"]).ToList();
            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"TableName", "myWorkflowStepPathEntity"},
                                                {"ProcessGUID", row["ProcessGUID"].ToString()},
                                                {"ProcessName", row["ProcessName"].ToString()}
                                            }).ToList();

            return new CheckResult
            {
                ErrorList = errorList,
                RepairParamList = repairParamList,
                ErrorCode = "10202"
            };
        }

        public object Repair(object input)
        {
            throw new NotImplementedException();
        }
    }
}
