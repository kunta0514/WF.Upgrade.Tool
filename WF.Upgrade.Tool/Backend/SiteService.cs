using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WF.Upgrade.Model;

namespace WF.Upgrade.Tool.Backend
{
    public class SiteService
    {

        public SiteInfo GetSiteInfoEntity()
        {
            return  WF.Upgrade.Public.Site.GetSiteInfoEntity();
        }
        public string GetSiteInfo()
        {
            return WF.Upgrade.Public.Site.GetSiteInfo();
        }

        public string SaveSiteInfo(string data)
        {
            return WF.Upgrade.Public.Site.SaveSiteInfo(data);
        }

        public string TestDBConn(string data)
        {
            return WF.Upgrade.Public.Site.TestDBConn(data);
        }
    }

    
}
