using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Public
{
    public class InitDb
    {
        public static string ConnectionStr()
        {
            var info = Site.GetSiteInfoEntity();
            if (info != null
               && !string.IsNullOrEmpty(info.DBServerName)
               && !string.IsNullOrEmpty(info.DBName)
               && !string.IsNullOrEmpty(info.DBUserName)
               && !string.IsNullOrEmpty(info.DBSaPassword))
            {
                try
                {
                    var strConnection = string.Format("server={0};uid={1};pwd={2};database={3}", info.DBServerName, info.DBUserName, info.DBSaPassword, info.DBName);


                    return strConnection;
                }
                catch
                {
                    return "";
                }
            }
            return "";
        }

       
    }
}
