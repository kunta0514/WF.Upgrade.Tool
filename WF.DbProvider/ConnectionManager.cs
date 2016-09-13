using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace WF.DbProvider
{
    /// <summary>
    /// 连接管理器
    /// </summary>
   internal class ConnectionManager:IDisposable
    {
        private static SqlTransaction _transaction;  //SQL事务
        private static SqlConnection _connection;    //SQL连接
       private static string _connectionString;     //连接字符串

       public ConnectionManager(string connectionString) {
           if (string.IsNullOrEmpty(connectionString)) {
               throw new ArgumentException(connectionString);
           }
           if (_connectionString != connectionString)
           {
               _connection = null;
               _transaction = null;
           }
           _connectionString = connectionString;
       }
       public T ExecuteCommand<T>(SqlCommand command, Func<SqlCommand, T> func)
       {
           if (command == null)
               throw new ArgumentNullException("command");
           // 打开连接，并根据需要开启事务
           if (_connection == null) {
               _connection = new SqlConnection(_connectionString);
               _connection.Open();
           }
           //开始事务
           if (_transaction == null) {
              _transaction = _connection.BeginTransaction();
           }
           //设置数据库连接
           command.Connection = _connection;
           command.Transaction = _transaction;
           try
           {
               T result = func(command);
               return result;
           }
           catch (Exception ex){
               
               throw new Exception(ex.Message, ex);
           }
           finally {
               // 让命令与连接，事务断开，避免这些资源外泄。
               command.Connection = null;
               command.Transaction = null;
           }
       }
       public void Dispose() { 
       
       }
    }
}
