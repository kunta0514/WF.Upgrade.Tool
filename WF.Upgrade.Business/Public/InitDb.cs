using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Business
{
    //TODO::需要改造，数据库连接是从UI上来的，但是需要改成读取方式
    public class InitDb
    {
        public static string ConnectionStr()
        {
            //var info = Site.GetSiteInfoEntity();
            //if (info != null
            //   && !string.IsNullOrEmpty(info.DBServerName)
            //   && !string.IsNullOrEmpty(info.DBName)
            //   && !string.IsNullOrEmpty(info.DBUserName)
            //   && !string.IsNullOrEmpty(info.DBSaPassword))
            //{
            //    try
            //    {
            //        var strConnection = string.Format("server={0};uid={1};pwd={2};database={3}", info.DBServerName, info.DBUserName, info.DBSaPassword, info.DBName);


            //        return strConnection;
            //    }
            //    catch
            //    {
            //        return "";
            //    }
            //}
            return "";
        }

       
    }
}
