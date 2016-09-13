using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WF.Upgrade.Public;

namespace WF.DbProvider
{
    /// <summary>
    /// 连接范围
    /// </summary>
    public sealed class ConnectionScope:IDisposable
    {
        private static string s_connectionString;

        [ThreadStatic]
        private static ConnectionManager _connectionManager;
        
        public ConnectionScope() {
            if (string.IsNullOrEmpty(s_connectionString)) {
                s_connectionString = InitDb.ConnectionStr();
            }
            Init(s_connectionString);
        }
        /// <summary>
        /// 实例化数据库连接
        /// </summary>
        /// <param name="connectionString">数据库连接地址</param>
        public ConnectionScope(string connectionString) {
            if (connectionString == s_connectionString) {
                return;
            }
            if (!string.IsNullOrEmpty(connectionString))
            {
                s_connectionString = connectionString;
                _connectionManager = null;
            }
            Init(s_connectionString);
        }
       
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionString"></param>
        private void Init(string connectionString)
        {
            if (_connectionManager == null)
            {
                _connectionManager = new ConnectionManager(connectionString);
            }
        }

        internal ConnectionManager Current { get { return _connectionManager; } }
        public void Dispose() { 
        }
    }
}
