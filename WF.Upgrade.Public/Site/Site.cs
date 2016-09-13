using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WF.Upgrade.Model;

namespace WF.Upgrade.Public
{
    public class Site
    {

        private static readonly string _configPath = Utility.GetBaseDirectory() + "Config/SiteInfo.config";

        public static SiteInfo SiteInfoEntity;
        public static SiteInfo GetSiteInfoEntity()
        {
            SiteInfo config;
            
            if (!File.Exists(_configPath))
            {
                return null;
            }

            using (var fs = new FileStream(_configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var xsl = new XmlSerializer(typeof(SiteInfo));
                config = (SiteInfo)xsl.Deserialize(fs);
            }
            SiteInfoEntity = config;
            return config;
        }
        public static string GetSiteInfo()
        {
            var config = GetSiteInfoEntity();
            return JsonConvert.SerializeObject(config);
        }

        public static string SaveSiteInfo(string data)
        {
            try
            {
                var info = JsonConvert.DeserializeObject<SiteInfo>(data);
                
                if (!File.Exists(_configPath))
                {
                    return "站点配置文件不存在！";
                }

                var xml = new XmlDocument();

                xml.Load(_configPath);

                xml.SelectSingleNode("SiteInfo/CustomCode").InnerText = info.CustomCode;
                xml.SelectSingleNode("SiteInfo/CustomName").InnerText = info.CustomName;
                xml.SelectSingleNode("SiteInfo/ERPUrl").InnerText = info.ERPUrl;
                xml.SelectSingleNode("SiteInfo/WFUrl").InnerText = info.WFUrl;
                xml.SelectSingleNode("SiteInfo/ERPVersion").InnerText = info.ERPVersion;
                xml.SelectSingleNode("SiteInfo/DBServerName").InnerText = info.DBServerName;

                xml.SelectSingleNode("SiteInfo/DBName").InnerText = info.DBName;
                xml.SelectSingleNode("SiteInfo/DBUserName").InnerText = info.DBUserName;
                xml.SelectSingleNode("SiteInfo/DBSaPassword").InnerText = info.DBSaPassword;
                xml.SelectSingleNode("SiteInfo/UpFilesAddress").InnerText = info.UpFilesAddress;
                xml.SelectSingleNode("SiteInfo/WFAddress").InnerText = info.WFAddress;

                xml.Save(_configPath);


                return string.Empty;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string TestDBConn(string data)
        {
            StringBuilder sb=new StringBuilder();
            string saveRet = SaveSiteInfo(data);

            if (!string.IsNullOrEmpty(saveRet))
            {
                return saveRet;
            }
            var conn = new SqlConnection(InitDb.ConnectionStr());
            try
            {                
                conn.Open();               
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();

                sb.Append("数据库连接失败！\r\n" + ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return sb.ToString();
        }


        public static string Test(SiteInfo info)
        {
            var sb =new StringBuilder();
            var upFilesAddress =new  DirectoryInfo(info.UpFilesAddress);

            if (!upFilesAddress.Exists)
            {
                sb.Append("UpFiles目录不存在！\r\n");
            }

            var wFAddress = new DirectoryInfo(info.WFAddress);
            {
                if (!wFAddress.Exists)
                {
                    sb.Append("工作流站点目录不存在！\r\n");
                }
            }

            var conn = new SqlConnection(InitDb.ConnectionStr());
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();

                sb.Append("数据库连接失败！\r\n");
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return sb.ToString();
        }
    }
}
