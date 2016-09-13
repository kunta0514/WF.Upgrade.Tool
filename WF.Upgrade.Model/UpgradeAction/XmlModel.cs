using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Model.UpgradeAction
{
    public class XmlModel
    {
        /// <summary>
        /// 配置失败时消息记录
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 节点路径
        /// </summary>
        public string NodePath { get; set; }

        /// <summary>
        /// 是否属性类配置
        /// </summary>
        public bool IsAttribute { get; set; }

        /// <summary>
        /// 是否递归创建节点
        /// </summary>
        public bool IsCreate { get; set; }

        /// <summary>
        /// 节点属性及属性值集合
        /// </summary>
        public NameValueCollection AttributeValue { get; set; }
    }
}
