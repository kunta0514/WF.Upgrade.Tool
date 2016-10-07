using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WF.Upgrade.Framework;

namespace WF.Upgrade.Business
{
    public class ProcessModuleForm : ICheckRule
    {

        //public ProcessModuleForm()
        //{

        //}

        //public static List<Result> CheckForm()
        //{
        //    List<Result> rtnlist = new List<Result>();

        //    string sql = @"select * from myWorkflowProcessModule where IsActive = 1";
        //    List<ProcessMoule> pmList = Mysoft.Interface.Extensions.DbProvider.CPQuery.From(sql).ToList<Model.ProcessMoule>();
        //    List<string> pmGuidList = (from ProcessMoule pm in pmList select pm.ProcessGUID).ToList();

        //    Form_DB.Detection(pmGuidList);
        //    int intSum, intHowMany;
        //    do
        //    {
        //        intSum = Mysoft.FormDetectionTool.Business.Form_DB.getCurrSumDetection();
        //        intHowMany = Mysoft.FormDetectionTool.Business.Form_DB.getCurrHowManyDetection();
        //    } while (intSum > intHowMany);

        //    Mysoft.Interface.Extensions.Result obj = Form_DB.getDetection();
        //    rtnlist = (from Interface.Extensions.listInfo info in obj.Lists select new Result { Guid = new Guid(info.GUID),Name = info.Name,TableName= "myWorkflowProcessModuleDefinition",ExInfo = info.Info }).ToList();
        //    return rtnlist;
        //}
        public CheckResult Check(object input)
        {
            throw new NotImplementedException();
        }

        public object Repair(object input)
        {
            throw new NotImplementedException();
        }
    }
}
