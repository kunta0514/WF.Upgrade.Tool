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
    [CheckRuleType("after-up")]
    [CheckRuleRemark("已启用的流程模版中没有业务对象、业务类型，请检查是否为通用审批流，可能会引起流程无法发起")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("已启用的流程模版中没有业务对象")]
    public class Module_BusinessObject : ICheckRule
    {
        public CheckResult Check(object input)
        {
            string sql = @"select * from myWorkflowProcessModule where BusinessTypeGUID is NULL AND BusinessType<>'' and IsActive = 1";
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
            //TODO:需要确认
            string strRepair =
                @"UPDATE a SET a.BusinessTypeGUID=b.BusinessTypeGUID FROM dbo.myWorkflowProcessModule a LEFT JOIN dbo.myworkflowbiztype b ON a.BusinessType=b.businesstypename
                WHERE a.BusinessType<>''";


            return null;
        }
    }

    [FromVersion("256")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("before-up")]
    [CheckRuleRemark("已启用的流程模版中:有业务对象,没有业务类型;，没有业务对象（包括业务对象被删除的情况）,有业务类型;导致模板无法发起、实例无法查看")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("已启用的流程模版中业务对象和业务类型不匹配")]
    public class BU_Module_BusinessObjWithBizType : ICheckRule
    {
        public CheckResult Check(object input)
        {
            StringBuilder strSql = new StringBuilder();
            //1、有业务对象 没有业务类型
            strSql.Append(@"SELECT  a.ProcessName ,
                                    a.ProcessGUID ,
                                    b.Name
                            FROM    dbo.myWorkflowProcessModule a
                                    LEFT JOIN dbo.myWorkflowBusinessType b ON b.BusinessTypeGUID = a.BusinessTypeGUID
                            WHERE   a.BusinessTypeGUID IS NOT NULL AND ISNULL(BusinessType,'')=''");
            //--2、没有业务对象（包括业务对象被删除的情况） 有业务类型
            strSql.Append(@"UNION
                            SELECT  a.ProcessName ,
                                    a.ProcessGUID ,
                                    b.Name
                            FROM    dbo.myWorkflowProcessModule a
                                    LEFT JOIN dbo.myWorkflowBusinessType b ON b.BusinessTypeGUID = a.BusinessTypeGUID
		                            WHERE BusinessType<>'' AND ISNULL(b.Name,'')=''");

            DataTable dt = CPQuery.From(strSql.ToString()).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "已启用的流程模版中业务对象和业务类型不匹配：" + row["ProcessName"]).ToList();
            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"TableName", "myWorkflowProcessModule"},
                                                {"ProcessName", row["ProcessName"].ToString()},
                                                {"ProcessGUID", row["ProcessGUID"].ToString()}
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
            throw new NotImplementedException();
        }
    }

    [FromVersion("256")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("after-up")]
    [CheckRuleRemark("已启用的流程模版中:有业务对象,没有业务类型;，没有业务对象（包括业务对象被删除的情况）,有业务类型;导致模板无法发起、实例无法查看")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("已启用的流程模版中业务对象和业务类型不匹配")]
    public class AU_Module_BusinessObjWithBizType : ICheckRule
    {
        public CheckResult Check(object input)
        {
            StringBuilder strSql = new StringBuilder();
            //1、找不到匹配的业务对象
            strSql.Append(@"SELECT  a.ProcessName ,
                                    a.ProcessGUID ,
                                    b.Name
                            FROM    dbo.myWorkflowProcessModule a
                                    LEFT JOIN dbo.myWorkflowBusinessObject b ON b.BusinessObjectGUID = a.BusinessObjectGUID
                            WHERE   a.BusinessObjectGUID IS NOT NULL
                                    AND ISNULL(Name, '') = ''");

            DataTable dt = CPQuery.From(strSql.ToString()).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "已启用的流程模版中找不到对应的业务对象：" + row["ProcessName"]).ToList();
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
            throw new NotImplementedException();
        }
    }
}
