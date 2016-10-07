using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WF.Upgrade.Framework;

namespace WF.Upgrade.Business
{
    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("before-up")]
    [CheckRuleRemark("检索有非标准(包括放弃的)的责任人定义，请与一线确认是否需要保留")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("非标准产品的责任人解析规则")]

    public class Auditor: ICheckRule
    {
        public CheckResult Check(object input)
        {
            return CheckAuditorDefine();
        }

        public object Repair(object input)
        {
            List<Dictionary<string, string>> repairParamList = input as List<Dictionary<string, string>>;
            return null;
        }


        private CheckResult CheckAuditorDefine()
        {
            string sql = @"select * from myWorkflowAuditorDefine where auditorName not in (
                        '[发起人]',
                        '[发起人直接上级]',
                        '[发起部门负责人]',
                        '[发起部门上一级部门负责人]',
                        '[发起部门上两级部门负责人]',
                        '[事项费用负责人]',
                        '[部门费用负责人]',
                        '[费用使用人]',
                        '[计划汇报审批人]',
                        '本公司',
                        '1级公司',
                        '2级公司',
                        '3级公司',
                        '[已完成步骤责任人]',
                        '[已完成步骤处理人]',
                        '[已完成步骤协商人]',
                        '[已完成步骤抄送人]',
                        '[流程监控人]',
                        '全集团',
                        '全集团专业线',
                        '本地区',
                        '本地区专业线',
                        '本公司',
                        '本公司专业线',
                        '本部门')";
            //foreach (var item in CPQuery.From(sql).ToList<object>())
            //{
            //    rtnlist.Add(new CheckResult { Name = item.AuditorName, Guid = new Guid(item.AuditorScope), TableName = "myWorkflowAuditorDefine", ExInfo = "发现有非标准的责任人定义，请与一线确认是否需要保留", ErrorCode = "10901" });
            //}
            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "非标准的责任人定义：" + row["AuditorName"]).ToList();
            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"TableName", "myWorkflowAuditorDefine"},
                                                {"AuditorName", row["AuditorName"].ToString()},
                                                {"AuditorScope", row["AuditorScope"].ToString()}
                                            }).ToList();

            return new CheckResult
            {
                ErrorList = errorList,
                RepairParamList = repairParamList,
                ErrorCode = "10901"
            };
        }
    }

    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("after-up")]
    [CheckRuleRemark("模板中使用了未定义的责任人,需要通知一线调整，会导致流程无法发起。")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("模板中使用了未定义的责任人")]

    public class AuditorScropNotExits : ICheckRule
    {
        public CheckResult Check(object input)
        {
            return CheckAuditorDefine();
        }

        public object Repair(object input)
        {
            List<Dictionary<string, string>> repairParamList = input as List<Dictionary<string, string>>;
            return null;
        }


        private CheckResult CheckAuditorDefine()
        {
            string sql = string.Format(@"SELECT a.ProcessGUID,a.ProcessName,b.StepName,c.AuditorName FROM dbo.myWorkflowProcessModule a  JOIN dbo.myWorkflowStepModuleDefinition b ON a.ProcessGUID=b.StepDefinitionGUID 
                JOIN dbo.myWorkflowBandModuleDefinition c  ON b.StepDefinitionGUID=c.BandDefinitionGUID
                LEFT JOIN dbo.myWorkflowAuditorDefine d ON c.AuditorScope=d.auditorScope AND d.auditorType = c.AuditorType
                WHERE d.auditorScope IS NULL");

            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "模板中使用了未定义的责任人：" + row["ProcessName"]).ToList();
            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"TableName", "myWorkflowProcessModuleDefinition"},
                                                {"ProcessName", row["ProcessName"].ToString()},
                                                {"ProcessGUID", row["ProcessGUID"].ToString()}
                                            }).ToList();

            return new CheckResult
            {
                ErrorList = errorList,
                RepairParamList = repairParamList,
                ErrorCode = "10901"
            };
        }
    }
}
