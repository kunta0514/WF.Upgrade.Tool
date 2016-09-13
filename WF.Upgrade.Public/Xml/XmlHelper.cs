using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;
using WF.Upgrade.Model.UpgradeAction;

namespace WF.Upgrade.Public.Xml
{
    public class XmlHelper
    {
        private string _xmlPath;

        private XmlDocument _xmlDoc;

        public XmlHelper(string xmlPath)
        {
            _xmlPath = xmlPath;
            _xmlDoc = new XmlDocument();
            _xmlDoc.Load(_xmlPath);
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="xmlModel"></param>
        public void CreateNode(List<XmlModel> list)
        {
            foreach (XmlModel xmlModel in list)
            {
                string strFindNode = GetAttributeValue(xmlModel.AttributeValue);
                XmlNode node;

                //属性类
                if (xmlModel.IsAttribute)
                {
                    node = _xmlDoc.SelectSingleNode(xmlModel.NodePath);
                    if (node != null)
                    {
                        SetAttribute((XmlElement)node, xmlModel.AttributeValue);
                    }
                    else
                    {
                        xmlModel.Message = "失败：节点" + xmlModel.NodePath + "不存在！";
                    }
                }
                else
                {
                    node = _xmlDoc.SelectSingleNode(xmlModel.NodePath + strFindNode);
                    if (node == null)
                    {
                        int index = xmlModel.NodePath.LastIndexOf("/");
                        string nodeName = xmlModel.NodePath.Remove(0, index + 1);
                        string parentPath = xmlModel.NodePath.Remove(index);

                        XmlElement xmlElement = _xmlDoc.CreateElement(nodeName);
                        SetAttribute(xmlElement, xmlModel.AttributeValue);

                        XmlNode parentNode = _xmlDoc.SelectSingleNode(parentPath);
                        if (parentNode != null)
                        {
                            parentNode.AppendChild(xmlElement);
                        }
                        else
                        {
                            xmlModel.Message = "失败：父节点" + parentPath + "不存在！";
                        }
                    }
                }
            }

            if (list != null && list.Count > 0)
            {
                //查询list中Message不为空的XmlModel
                XmlModel xmlModel = list.FirstOrDefault(x => !string.IsNullOrEmpty(x.Message));

                if (xmlModel == null)
                {
                    _xmlDoc.Save(_xmlPath);
                    Console.WriteLine("配置成功！");
                }
                else
                {
                    Console.WriteLine("配置失败！");
                    Console.WriteLine(xmlModel.Message);
                }
                Console.ReadLine();
            }
        }

        /// <summary>
        /// 获取全部属性信息
        /// </summary>
        /// <param name="nv"></param>
        /// <returns>格式：[@verb='POST,GET'][@path='ajax.axd'][@type='Mysoft.MAP2.Ajax.WFAjaxHandlerFactory, Mysoft.MAP2.Ajax']</returns>
        private string GetAttributeValue(NameValueCollection nv)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in nv)
            {
                sb.Append("[@");
                sb.Append(key);
                sb.Append("='");
                sb.Append(nv[key]);
                sb.Append("']");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 配置节点属性
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <param name="attributeValue"></param>
        private void SetAttribute(XmlElement xmlElement, NameValueCollection attributeValue)
        {
            foreach (string key in attributeValue.Keys)
            {
                xmlElement.SetAttribute(key, attributeValue[key]);
            }
        }
    }
}