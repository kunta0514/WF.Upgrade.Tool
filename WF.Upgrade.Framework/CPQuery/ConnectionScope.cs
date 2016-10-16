using System;

namespace WF.Upgrade.Framework.CPQuery
{
    /// <summary>
    /// 连接范围
    /// </summary>
    public sealed class ConnectionScope:IDisposable
    {
        private static string s_connectionString;

        internal static void SetDefaultConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            s_connectionString = connectionString;
        }
        internal static string GetDefaultConnectionString()
        {
            return s_connectionString;
        }

        [ThreadStatic]
        private static ConnectionManager _connectionManager;
        
        public ConnectionScope() {
            if (string.IsNullOrEmpty(s_connectionString)) {
                //TODO:数据库连接外置到读取配置文件，升级数据库的连接来自于UI
                //s_connectionString = InitDb.ConnectionStr();
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
