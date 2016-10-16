using System;
using System.Runtime.CompilerServices;

namespace WF.Upgrade.Framework.CPQuery
{
    public static class Initializer
    {

        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        /// <param name="connectionString"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void init_connectionString(string connectionString)
        {
            // 设置默认的连接字符串。
            ConnectionScope.SetDefaultConnection(connectionString);
        }
    }
}
