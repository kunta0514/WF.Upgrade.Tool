using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WF.Upgrade.Model;
using WF.Upgrade.Model.CustomAttribute;
using WF.Upgrade.Public;
using System.Data;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using WF.DbProvider;

namespace WF.CheckRule.Business.Form
{
    [FromVersion("305sp1,306,307,307sp1,307sp2")]
    [ToVersion("306,307,307sp1,307sp2,308")]
    [CheckRuleType("after-up")]
    [CheckRuleRemark("流程模版存在字段在业务对象中不存在,DomainXML中存在重复的字段")]
    [CheckRuleKind("数据检测")]
    [CheckRuleName("流程模版中DomainXML存在异常")]
    public class BusinessObject : ICheckRule
    {
        public CheckResult Check(object input)
        {
            //Result rs = new Result();
            //List<Result> rtnlist = new List<Result>();
            //流程模板根目录
            //TODO::文件路径从工作流物理文件地址读取
            //string rootPath = Site.SiteInfoEntity.UpFilesAddress;
            string rootPath = @"E:\workflow\07 项目ERP\04 重庆融汇\WorkflowDoc\流程模板";
            if (!rootPath.EndsWith("\\"))
            {
                rootPath = rootPath + "\\";
            }
            BTDomainXML btx = new BTDomainXML();
            string sql = @"SELECT md.BusinessObjectGUID,bo.Name as BusinessObjectName, ProcessDefinitionGUID,ProcessName,PublishVersionName,BT_DomainXML,DocumentPath,dbo.fn_GetProcessKindPath(ProcessKindGUID) AS ProcessKindPath,VersionStatus,case when VersionStatus =2 then '已发布' when VersionStatus =3 then '历史模板' end as VersionStatusText FROM dbo.myWorkflowProcessModuleDefinition md INNER JOIN myWorkflowDocument wfd
                                      ON md.D_Document = wfd.DocumentGUID
                                      INNER JOIN myWorkflowBusinessObject bo
                                      ON md.BusinessObjectGUID = bo.BusinessObjectGUID
                                      WHERE VersionStatus IN (2,3)";

            //保存结果
            List<string> resultList = new List<string>();
            //保存业务对象信息业务对象guid,业务对象名称
            List<string> boListInfo = new List<string>();
            //保存业务对象信息业务对象guid,业务对象名称
            //复件\s * (\([0 - 9]{ 1,2}\))*\s*
            List<string> boDistinctNameList = new List<string>();
            string btDomainXML, documentPath, formHTML, isRightStr, exInfo;

            DataTable dt;
            try
            {
                dt = CPQuery.From(sql).FillDataTable();



                List<Dictionary<string, string>> listDic = Utility.ToListDic(dt);

                List<string> errorList = new List<string>();
                List<Dictionary<string, string>> repairParamList = new List<Dictionary<string, string>>();

                foreach (var item in listDic)
                {
                    //开始扫描
                    btDomainXML = item["BT_DomainXML"];
                    documentPath = item["DocumentPath"];
                    if (string.IsNullOrEmpty(item["DocumentPath"])) continue;
                    documentPath = rootPath + documentPath;

                    //如果输入路径字符串不带"\"结尾，则给路径加上"\"
                    if (string.IsNullOrEmpty(btDomainXML) == false)
                    {
                        formHTML = GetFileInfo(documentPath);
                        if (string.IsNullOrEmpty(formHTML)) continue;

                        isRightStr = btx.EnsureHtmlFormFieldsAllEnable(formHTML, btDomainXML);
                        if (!string.IsNullOrEmpty(isRightStr))
                        {
                            boListInfo.Add(Convert.ToString(item["BusinessObjectGUID"]) + "," + Convert.ToString(item["BusinessObjectName"]));
                            boDistinctNameList.Add(Regex.Replace(Convert.ToString(item["BusinessObjectName"]), @"复件\s*(\([0-9]{1,2}\))*\s*", ""));
                            //文字结果
                            exInfo = "模板状态：" + Convert.ToString(item["VersionStatusText"]) + "业务对象guid：" + Convert.ToString(item["BusinessObjectGUID"]) + ",ProcessDefinitionGUID:" + Convert.ToString(item["ProcessDefinitionGUID"]) + ",ProcessName:" + Convert.ToString(item["ProcessName"]) + ",流程模板文档路径：" + documentPath + ",第一个匹配的错误字段：" + isRightStr + ",流程模板的路径：" + Convert.ToString(item["ProcessKindPath"]);
                            errorList.Add(exInfo);
                            repairParamList.Add(new Dictionary<string, string>
                                                {
                                                    {"TableName", "myWorkflowProcessModuleDefinition"},
                                                    {"BusinessObjectGUID", item["BusinessObjectGUID"]},
                                                    {"ProcessDefinitionGUID", Convert.ToString(item["ProcessDefinitionGUID"])},
                                                    {"ProcessName", item["ProcessName"]},
                                                    {"ProcessKindPath", item["ProcessDefinitionGUID"]}
                                                });
                            //rtnlist.Add(new Result { Name = item.BusinessObjectName, Guid = new Guid(item.BusinessObjectGUID), TableName = "myWorkflowBusinessObject", ExInfo = exInfo, ErrorCode = "10103" });
                        }

                    }
                }

                return new CheckResult
                       {
                           ErrorList = errorList,
                           RepairParamList = repairParamList,
                           ErrorCode = "10103"
                       };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private  string GetFileInfo(string modulePath)
        {
            if (File.Exists(modulePath))
            {
                StreamReader sr = new StreamReader(modulePath, Encoding.Default);
                string formHtml = sr.ReadToEnd();
                sr.Close();
                return formHtml;
            }
            return string.Empty;
        }

        //public CheckResult Check2(object input)
        //{
        //    return checkBusinessObject();
        //}

        public object Repair(object input)
        {
            throw new NotImplementedException();
        }
    }
}
