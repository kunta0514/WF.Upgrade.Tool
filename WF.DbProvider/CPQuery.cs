using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace WF.DbProvider
{
    public sealed class CPQuery : IDbExecute
    {
        private SqlCommand _command = new SqlCommand();
        private StringBuilder _sb = new StringBuilder(512);
        private SqlCommand Command {
            get {
                _command.CommandText = _sb.ToString();
                return _command;
            }
        }
        internal SqlCommand GetCommand() {
            return this.Command;
        }

        internal CPQuery(string text)
        {
            this.AddSqlText(text);
        }

        private void AddSqlText(string s)
        {
            if (string.IsNullOrEmpty(s))
                return;
            _sb.Append(s);
        }

        #region 对外的接口

        public static CPQuery From(string parameterizedSQL)
        {
            return From(parameterizedSQL,null);
        }

        /// <summary>
        /// 通过参数化SQL、SqlParameter数组的方式，创建CPQuery实例
        /// </summary>
        /// <example>
        /// <para>下面的代码演示了通过参数化SQL、SqlParameter数组的方式，创建CPQuery实例的用法</para>
        /// <code>
        /// //声明参数数组
        /// SqlParameter[] parameters2 = new SqlParameter[2];
        /// parameters2[0] = new SqlParameter("@ProductID", SqlDbType.Int);
        /// parameters2[0].Value = 0;
        /// parameters2[1] = new SqlParameter("@ProductName", SqlDbType.VarChar, 50);
        /// parameters2[1].Value = "测试";
        /// //执行查询并返回实体
        /// Products product = CPQuery.From("SELECT * FROM Products WHERE ProductID = @ProductID AND ProductName=@ProductName", parameters2).ToSingle&lt;Products&gt;();
        /// </code>
        /// </example>
        /// <param name="parameterizedSQL">参数化的SQL字符串</param>
        /// <param name="parameters">SqlParameter参数数组</param>
        /// <returns>CPQuery对象实例</returns>
        public static CPQuery From(string parameterizedSQL, params SqlParameter[] parameters)
        {
            CPQuery query = new CPQuery(parameterizedSQL);
            if (parameters != null)
            {
                query._command.Parameters.AddRange(parameters);
            }
            return query;
        }

        public static CPQuery From(string parameterizedSQL, object replaces, params SqlParameter[] parameters)
        {
            List<SqlParameter> parameterList =new List<SqlParameter>(parameters);
            if (replaces != null) {
                PropertyInfo[] properties = replaces.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                int index = 1;
                // 为每个DbParameter赋值。
                foreach (PropertyInfo pInfo in properties) {
                    string pName = "{" + pInfo.Name + "}";
                    object pValue = pInfo.GetValue(replaces, null);
                    if (pValue != null) {
                        if (pValue is string) {
                            parameterizedSQL = parameterizedSQL.Replace(pName, pValue.ToString());
                        }
                        else if (pValue is ICollection)
                        {
                            StringBuilder sb = new StringBuilder(128);
                            foreach (object obj in pValue as ICollection)
                            {
                                string paramName = "@_param_" + index.ToString();
                                SqlParameter parameter = new SqlParameter(paramName, obj);
                                parameterList.Add(parameter);
                                if (sb.Length != 0)
                                {
                                    sb.Append(",");
                                }
                                sb.Append(paramName);
                                index++;
                            }
                            if( sb.Length == 0 ){
                                sb.Append("NULL");
                            }
                            parameterizedSQL = parameterizedSQL.Replace(pName, sb.ToString());
                        }
                    }
				}

			}
            return From(parameterizedSQL, parameterList.ToArray());
        }

        /// <summary>
        /// 执行命令,并返回影响函数
        /// </summary>
        /// <returns>影响行数</returns>
       public int ExecuteNonQuery()
       {
           return DbHelper.ExecuteNonQuery(this.GetCommand());
       }
       /// <summary>
       /// 执行命令,并将结果集填充到DataTable
       /// </summary>
       /// <returns>数据集</returns>
       public DataTable FillDataTable()
       {
           return DbHelper.ToDataTable(this.GetCommand());
       }

       /// <summary>
       /// 执行查询,并将结果集填充到DataSet
       /// </summary>
       /// <returns>数据集</returns>
       public DataSet FillDataSet()
       {
           return DbHelper.ToDataSet(this.GetCommand());
       }

       /// <summary>
       /// 执行命令,返回第一行,第一列的值,并将结果转换为T类型
       /// </summary>
       /// <typeparam name="T">返回值类型</typeparam>
       /// <returns>结果集的第一行,第一列</returns>
       public T ExecuteScalar<T>()
       {
           return DbHelper.ExecuteScalar<T>(this.GetCommand());
       }

       /// <summary>
       /// 执行命令,将结果集转换为实体集合
       /// </summary>
       /// <example>
       /// <para>下面的代码演示了如何返回实体集合</para>
       /// <code>
       /// List&lt;TestDataType&gt; list = CPQuery.Format("SELECT * FROM TestDataType").ToList&lt;TestDataType&gt;();
       /// </code>
       /// </example>
       /// <typeparam name="T">实体类型</typeparam>
       /// <returns>实体集合</returns>
       public List<T> ToList<T>() where T : class, new()
       {
           return DbHelper.ToList<T>(this.GetCommand());
       }
        #endregion
    }
}
