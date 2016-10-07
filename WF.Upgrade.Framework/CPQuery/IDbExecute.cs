using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace WF.Upgrade.Framework
{
    /// <summary>
    /// IDB执行接口
    /// </summary>
    public interface IDbExecute
    {
        /// <summary>
        /// 执行命令,并返回影响函数
        /// </summary>
        /// <returns>影响行数</returns>
        int ExecuteNonQuery();

        /// <summary>
        /// 执行命令,并将结果集填充到DataTable
        /// </summary>
        /// <returns>数据集</returns>
        DataTable FillDataTable();

        /// <summary>
        /// 执行命令,并将结果集填充到DataSet
        /// </summary>
        /// <returns>数据集</returns>
        DataSet FillDataSet();

        /// <summary>
        /// 执行命令,返回第一行,第一列的值,并将结果转换为T类型
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <returns>结果集的第一行,第一列</returns>
        T ExecuteScalar<T>();
    }
}
