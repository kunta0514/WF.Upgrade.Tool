using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WF.Upgrade.Model.DbUp;

namespace WF.Upgrade.Public
{
    public class DbUpInfo
    {
        private static readonly string _configPath = Utility.GetBaseDirectory() + "DbUpForm/UpDbInfo.config";


        public string GetUpDbInfo()
        {
            var info = GetUpDbInfoEntity();

            return JsonConvert.SerializeObject(info);
        }

        public DbUpSet GetUpDbInfoEntity()
        {
            try
            {
                DbUpSet config;

                if (!File.Exists(_configPath))
                {
                    return null;
                }

                using (var fs = new FileStream(_configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var xsl = new XmlSerializer(typeof (DbUpSet));
                    config = (DbUpSet) xsl.Deserialize(fs);
                }
                return config;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string SaveUpDbInfo(string data)
        {
            try
            {
                var info = JsonConvert.DeserializeObject<DbUpSet>(data);

                if (!File.Exists(_configPath))
                {
                    return "数据库升级配置文件不存在！";
                }

                var xml = new XmlDocument();

                xml.Load(_configPath);

                xml.SelectSingleNode("DbUpSet/old_server").InnerText = info.old_server;
                xml.SelectSingleNode("DbUpSet/old_dbname").InnerText = info.old_dbname;
                xml.SelectSingleNode("DbUpSet/old_username").InnerText = info.old_username;
                xml.SelectSingleNode("DbUpSet/old_password").InnerText = info.old_password;

                xml.SelectSingleNode("DbUpSet/new_server").InnerText = info.new_server;
                xml.SelectSingleNode("DbUpSet/new_dbname").InnerText = info.new_dbname;
                xml.SelectSingleNode("DbUpSet/new_username").InnerText = info.new_username;
                xml.SelectSingleNode("DbUpSet/new_password").InnerText = info.new_password;

                xml.SelectSingleNode("DbUpSet/erp_version").InnerText = info.erp_version.ToString();
                xml.SelectSingleNode("DbUpSet/erp_source").InnerText = info.erp_source;

                xml.Save(_configPath);


                return Test(info);

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static string Test(DbUpSet info)
        {
            var sb = new StringBuilder();

            try
            {
                var address = new DirectoryInfo(info.erp_source);

                if (!address.Exists)
                {
                    sb.Append("ERP目录不存在！\r\n");
                }
            }
            catch (Exception ex)
            {
                sb.Append("工作流目录格式错误！\r\n");
            }


            var strConnection = string.Format("server={0};uid={1};pwd={2};database={3}", info.old_server, info.old_username, info.old_password, info.old_dbname);

            var conn = new SqlConnection(strConnection);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();

                sb.Append("老数据库连接失败！\r\n");
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            strConnection = string.Format("server={0};uid={1};pwd={2};database={3}", info.new_server, info.new_username, info.new_password, info.new_dbname);

            conn = new SqlConnection(strConnection);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();

                sb.Append("新数据库连接失败！\r\n");
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
