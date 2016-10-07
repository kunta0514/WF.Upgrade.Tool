using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using WF.Upgrade.Framework;

namespace WF.Upgrade.Business
{
    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("all")]
    [CheckRuleRemark("表单修改业务数据链接扫描")]
    [CheckRuleKind("表单检测")]
    [CheckRuleName("表单修改业务数据链接扫描")]
    public class 表单修改业务数据链接扫描 : ICheckRule 
    {
        public CheckResult Check(object input)
        {

            var sql = @"SELECT  DISTINCT
                                bl.BusinessLinkGUID ,
                                bl.LinkAddress ,
                                bl.LinkName ,
                                bt.BusinessTypeGUID ,
                                bt.BusinessTypeName ,
                                bo.BusinessObjectGUID
                        FROM    dbo.myWorkflowBizType bt
                                LEFT JOIN dbo.myWorkflowBusinessObject bo ON bt.BusinessTypeGUID = bo.BusinessTypeGUID
                                LEFT  JOIN dbo.myWorkflowBusinessLink bl ON bl.BusinessObjectGUID = bo.BusinessObjectGUID
                        WHERE   bl.LinkType = 2
                                AND ISNULL(LinkAddress, '') = ''";



            DataTable dt = CPQuery.From(sql).FillDataTable();
            var errorList = dt.AsEnumerable().Select(row => "业务类型：" + row["BusinessTypeName"] + "，业务数据链接为空!").ToList();

            var repairParamList = dt.AsEnumerable().
                                Select(row => new Dictionary<string, string>
                                            {
                                                {"BusinessLinkGUID", row["BusinessLinkGUID"].ToString()},
                                                {"BusinessObjectGUID", row["BusinessObjectGUID"].ToString()},
                                                {"BusinessTypeGUID", row["BusinessTypeGUID"].ToString()},
                                                {"BusinessTypeName", row["BusinessTypeName"].ToString()},

                                            }).ToList();
            return new CheckResult
                   {
                       ErrorTitle = "表单业务数据链接为空",
                       ErrorList = errorList,
                       RepairParamList = repairParamList
                   };
        }

        public object Repair(object input)
        {
            List<Dictionary<string, string>> repairParamList = input as List<Dictionary<string, string>>;
            return null;
        }
    }


    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("all")]
    [CheckRuleRemark("HTML表单自定义JS扫描")]
    [CheckRuleKind("表单检测")]
    [CheckRuleName("HTML表单自定义JS扫描")]
    public class HTML表单自定义JS扫描:  ICheckRule
    {
        public CheckResult Check(object input)
        {

            var path = Site.GetSiteInfoEntity().UpFilesAddress +@"\WorkflowDoc\流程模板";
            var dirFilesInfo = new DirectoryInfo(path);
            var filesInfos = dirFilesInfo.GetFiles("*.htm", SearchOption.AllDirectories);
            var errorList = new List<string>();

            var repairParamList = new List<Dictionary<string,string>>();

            foreach (FileInfo info in filesInfos)
            {
                using (StreamReader sr = info.OpenText())
                {
                    var str = sr.ReadToEnd();

                    if (str.IndexOf("<script",StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        errorList.Add("流程模板文件：" + info.Name + "，包含JS语句！");
                        repairParamList.Add(new Dictionary<string, string>
                                            {
                                                {"流程模板名称", info.Name}
                                            });
                    }
                }
            }
          
            return new CheckResult
            {
                ErrorTitle = "HTML表单中包含js脚本,共扫描： "+ filesInfos.LongCount<FileInfo>()+"条！",
                ErrorList = errorList,
                RepairParamList = repairParamList
            };

        }

        public object Repair(object input)
        {
            List<Dictionary<string, string>> repairParamList = input as List<Dictionary<string, string>>;
            return null;
        }
    }


    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("after-up")]
    [CheckRuleRemark("流程实例业务类型为空")]
    [CheckRuleKind("流程模板检测")]
    [CheckRuleName("流程实例业务类型为空")]
    public class 流程实例业务类型为空 :  ICheckRule
    {

        public CheckResult Check(object input)
        {
            try
            {
                string sql = @"SELECT  ProcessGUID ,
                                    a.ProcessName ,
                                    a.BusinessTypeGUID ,
                                    a.BusinessType ,
                                    b.BusinessTypeName
                            FROM    dbo.myWorkflowProcessEntity a
                                    LEFT JOIN dbo.myWorkflowBizType b ON b.BusinessTypeGUID = a.BusinessTypeGUID
                            WHERE   b.BusinessTypeName IS  NULL";

                DataTable dt = CPQuery.From(sql).FillDataTable();
                var errorList = dt.AsEnumerable().Select(row => "流程名称：" + row["ProcessName"] + "，业务类型为空!").ToList();

                var repairParamList = dt.AsEnumerable().
                                         Select(row => new Dictionary<string, string>
                                                       {
                                                           {"ProcessGuid", row["ProcessName"].ToString()},

                                                       }).ToList();
                return new CheckResult
                {
                    ErrorTitle = "流程实例中没有业务类型",
                    ErrorList = errorList,
                    RepairParamList = repairParamList
                };
            }
            catch (Exception ex)
            {
                return new CheckResult
                {
                    ExceptionMessage = ex.Message
                };
            }
        }

        public object Repair(object input)
        {
            List<Dictionary<string, string>> repairParamList = input as List<Dictionary<string, string>>;
            return null;
        }

    }
}
