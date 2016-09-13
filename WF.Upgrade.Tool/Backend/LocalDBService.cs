using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Tool.Backend
{
    public class LocalDBService
    {
        private static string SQLCONN_PATH  = "Data Source =" + Environment.CurrentDirectory + "/mytool.db";

        public static void init() {
            SQLiteConnection conn = null;            
            conn = new SQLiteConnection(SQLiteHelper.connectionString);//创建数据库实例，指定文件位置  
            conn.Open();//打开数据库，若文件不存在会自动创建  
            conn.Close();
            if (!File.Exists(Environment.CurrentDirectory + @"\database\tool.db")){
                Init_table();
            }
            //
        }

        #region 本地数据库中业务相关代码
        private static void Init_table()
        {
            string sql = @"
DROP TABLE IF EXISTS 'main'.'p_check_rule';
CREATE TABLE 'p_check_rule' (
'id'  INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
'name'  TEXT,
'kind'  TEXT,
'type'  TEXT,
'remark'  TEXT,
'is_check'  INTEGER,
'begin_time'  TEXT,
'end_time'  TEXT,
'err_code',INTEGER
'err_msg' TEXT
'result_count'  INTEGER,
'check_count'  INTEGER
);
DROP TABLE IF EXISTS 'main'.'p_check_rule_result';
CREATE TABLE 'p_check_rule_result' (
'rule_id'  INTEGER NOT NULL,
'rule_name'  TEXT,
'ex_msg'  TEXT,
'repair_result'  INTEGER,
'repair_param'  TEXT,
PRIMARY KEY ('rule_id' ASC) );
";
            SQLiteHelper.ExecuteNonQuery(System.Data.CommandType.Text, sql);
        }
        #endregion
    }
}
