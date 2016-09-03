using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WF.DbProvider;
using WF.Upgrade.Tool.Backend;

namespace WF.Upgrade.Tool.FormCheck
{
    public class FormCheck
    {
        public List<WorkflowProcessModule> GetMyWorkflowProcessModuleList()
        {
            try
            {
                var sql = @"SELECT  
		                        ROW_NUMBER() OVER ( ORDER BY a.Application , a.DocumentType , a.PhoneFormMode ) AS Num ,
		                        a.ProcessGUID ,
                                a.ProcessName ,
                                a.ProcessKindName ,
		                        a.BusinessTypeGUID ,
                                a.DocumentType AS DocumentTypeInt ,
                                CASE a.PhoneFormMode
                                  WHEN '0' THEN 'Default'
                                  WHEN '1' THEN 'SingleLine'
                                  WHEN '2' THEN 'URL'
                                  WHEN '3' THEN 'Custom'
                                  ELSE 'Default'
                                END AS PhoneFormMode ,
                                a.PhoneFormDomainXml AS SingleFormDomainXML ,
                                a.PhoneUrl ,
                                a.BT_DomainXML AS DomainXml ,
                                a.IsActive ,
                                ISNULL(b.ApplicationName,'') AS ApplicationName ,
                                ISNULL(c.DocumentPath,'') AS DocumentPath
                        FROM    myWorkflowProcessModule a
                                LEFT  JOIN myApplication b ON a.Application = b.Application
                                LEFT JOIN myWorkflowDocument C ON a.D_Document = C.DocumentGUID
                        WHERE   a.DocumentType IN ( 0, 2 )
                        ORDER BY a.Application ,
                                a.DocumentType ,
                                a.PhoneFormMode";

                var list = CPQuery.From(sql).ToList<WorkflowProcessModule>();

           

                return list;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public string GetHtmlContent(string path)
        {

            string htmlPath = WF.Upgrade.Public.Site.GetSiteInfoEntity().UpFilesAddress + @"\WorkflowDoc\流程模板\" + path;

            if (!File.Exists(htmlPath))
            {
                return null;
            }
            var info = new FileInfo(htmlPath);


            using (StreamReader sr = info.OpenText())
            {
                var str = sr.ReadToEnd();

                return str;
            }
        }
    }
}
