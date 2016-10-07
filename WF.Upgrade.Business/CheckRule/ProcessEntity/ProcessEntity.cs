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
    //[CheckRuleType("after-up")]
    //[CheckRuleRemark("流程实例没有对应业务类型，请修复数据，会引起分类中出现其他选项")]
    //[CheckRuleKind("数据检测")]
    //[CheckRuleName("流程实例没有对应业务类型")]
    public class ProcessEntity : ICheckRule
    {
        public CheckResult Check(object input)
        {
            string sql = @"select ProcessGUID,a.ProcessName,a.BusinessTypeGUID , a.BusinessType,b.BusinessTypeGUID,b.BusinessTypeName 
                        FROM dbo.myWorkflowProcessEntity a LEFT JOIN dbo.myWorkflowBizType b ON b.BusinessTypeGUID = a.BusinessTypeGUID
                        WHERE b.BusinessTypeName IS  NULL AND BusinessType<>''";

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

    //[FromVersion("305sp1,306,307,307sp1,307sp2")]
    //[ToVersion("306,307,307sp1,307sp2,308")]
    //[CheckRuleType("all")]
    //[CheckRuleRemark("流程实例没有流程模板，请修复数据，会引起分类中出现其他选项")]
    //[CheckRuleKind("数据检测")]
    //[CheckRuleName("流程实例没有对应流程模板")]
    public class ProcessEntityNoModule : ICheckRule
    {
        public CheckResult Check(object input)
        {
            string sql = @"SELECT  a.ProcessName,a.ProcessModuleGUID
                        FROM    dbo.myWorkflowProcessEntity a
                        WHERE   ProcessModuleGUID NOT IN (
                                SELECT DISTINCT
                                        ProcessGUID
                                FROM    dbo.myWorkflowProcessModule )";

            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "流程实例没有对应业务类型：" + row["ProcessName"]).ToList();
            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"TableName", "myWorkflowProcessEntity"},
                                                {"ProcessModuleGUID", row["ProcessModuleGUID"].ToString()},
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
            throw new NotImplementedException();
        }
    }

    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("after-up")]
    [CheckRuleRemark("流程实例没有对应SiteName,打开审批页面报错")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("流程实例没有对应SiteName")]
    public class ProcessEntityNoSiteName : ICheckRule
    {
        public CheckResult Check(object input)
        {
            string sql = @"SELECT ProcessGUID,ProcessName FROM dbo.myWorkflowProcessEntity WHERE ISNULL(SiteName,'')=''";

            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "流程实例没有对应SiteName：" + row["ProcessName"]).ToList();
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
            throw new NotImplementedException();
        }
    }

    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("after-up")]
    [CheckRuleRemark("流程代办等标签页的流程分类总数和列表数量不一致")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("流程代办等标签页的流程分类总数和列表数量不一致")]
    public class ProcessEntityTypeNotMatchTypeGUID : ICheckRule
    {
        public CheckResult Check(object input)
        {
            string sql = @"SELECT ProcessGUID,ProcessName FROM dbo.myWorkflowProcessEntity WHERE BusinessTypeGUID IS NOT NULL AND BusinessType=''";

            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "流程代办等标签页的流程分类总数和列表数量不一致：" + row["ProcessName"]).ToList();
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
            throw new NotImplementedException();
        }
    }
}
