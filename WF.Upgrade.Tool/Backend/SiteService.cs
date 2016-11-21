using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WF.Upgrade.Model;
using Newtonsoft.Json.Linq;

namespace WF.Upgrade.Tool.Backend
{
    public class SiteService
    {

        //public SiteInfo GetSiteInfoEntity()
        //{
        //    return  WF.Upgrade.Public.Site.GetSiteInfoEntity();
        //}
        //public string GetSiteInfo()
        //{
        //    return WF.Upgrade.Public.Site.GetSiteInfo();
        //}

        //public string SaveSiteInfo(string data)
        //{
        //    return WF.Upgrade.Public.Site.SaveSiteInfo(data);
        //}

        //public string TestDBConn(string data)
        //{
        //    return WF.Upgrade.Public.Site.TestDBConn(data);
        //}
        private static readonly string _configPath = Environment.CurrentDirectory + @"\Config\site.json";

        public static JObject GetSiteInfo()
        {
            if (!File.Exists(_configPath))
            {
                return null;
            }
            var s = File.ReadAllText(_configPath);

            var m = JsonConvert.DeserializeObject<JObject>(s);

            //m["CustomCode"] = "localhost333";


            //File.WriteAllText(_configPath,JsonConvert.SerializeObject(m));

            return m;
        }

        public bool SaveSiteInfo(string data)
        {
            try {

                var save_data = JsonConvert.DeserializeObject<JObject>(data);
                File.WriteAllText(_configPath, JsonConvert.SerializeObject(save_data));
                return true;
            }
            catch {
                return false;
            }
        }
    }

    
}
