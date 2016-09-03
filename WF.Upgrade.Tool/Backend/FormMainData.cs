using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using WF.DbProvider;
using WF.Upgrade.Model;
using WF.Upgrade.Public;


namespace WF.Upgrade.Tool.Backend
{
    public class FormMainData
    {

        public FormMainData()
        {
            new ConnectionScope(InitDb.ConnectionStr());
        }

        private static List<WorkflowProcessModule> ModuleList;

        public string GetMyWorkflowProcessModuleList()
        {
            var dicList = new FormCheck.FormCheck().GetMyWorkflowProcessModuleList();
            ModuleList = dicList;
            return JsonConvert.SerializeObject(dicList);
        }

        private WorkflowProcessModule ProcessInfo;
        public string CheckForm(string processModuleGuid)
        {
            try
            {
                ProcessInfo = ModuleList.Where(model => model.ProcessGUID == processModuleGuid).FirstOrDefault();


                var result = new CheckResult
                             {
                                 CheckRuleName = "CheckForm",
                                 ErrorList = new List<string>(),
                                 RepairParamList = new List<Dictionary<string, string>>()
                             };

                ProcessInfo.htmlContent = new FormCheck.FormCheck().GetHtmlContent(ProcessInfo.DocumentPath);
                if (string.IsNullOrEmpty(ProcessInfo.htmlContent))
                {
                    result.ErrorList.Add("未设置对应HTML展示内容！");
                    return JsonConvert.SerializeObject(result);
                }
                if (string.IsNullOrEmpty(ProcessInfo.DomainXml))
                {
                    result.ErrorList.Add("DomainXml为空！");
                    return JsonConvert.SerializeObject(result);
                }

                Detection(ProcessInfo, result);
                RegularExpressionRulesList(ProcessInfo, result);

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(ProcessInfo.htmlContent);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(ProcessInfo.DomainXml);


                //找到所有的input
                HtmlNodeCollection inputs = htmlDoc.DocumentNode.SelectNodes("//input[@dm_type]");
                if (inputs != null)
                {
                    List<Dictionary<string, object>> domainList = new List<Dictionary<string, object>>();
                    foreach (HtmlNode input in inputs)
                    {
                        domainList.Add(GetInputInfo(input, result));
                    }

                    InputAttrList(ProcessInfo, xmlDoc, domainList, result);
                }

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                var data =  new CheckResult
                {
                    CheckRuleName = "CheckForm",
                    ErrorList = new List<string>{ex.Message},
                    RepairParamList = new List<Dictionary<string, string>>()
                };

                return JsonConvert.SerializeObject(data);
            }
        }
        /// <summary>
        /// 循环验证input中所有属性与规则是否正确
        /// </summary>
        /// <param name="ProcessInfo"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="inputAttrList"></param>
        /// <param name="obj"></param>
        public void InputAttrList(WorkflowProcessModule info, System.Xml.XmlDocument xmlDoc, List<Dictionary<string, object>> inputAttrList, CheckResult result)
        {
            foreach (Dictionary<string, object> inputAttr in inputAttrList)
            {
                int ItemI = xmlDoc.SelectNodes("/BusinessType/Item/Domain[@name='" + inputAttr["dm_name_show_temp"].ToString() + "']").Count;
                int GroupItemI = xmlDoc.SelectNodes("/BusinessType/Item/Group/Item/Domain[@name='" + inputAttr["dm_name_show_temp"].ToString() + "']").Count;
                if (ItemI > 1 || GroupItemI > 1)
                {
                    result.ErrorList.Add("DomainXml存在重复节点！");
                }
                //判定input中所有的值是否都在业务域中
                if (ItemI == 0 && GroupItemI == 0)
                {
                    result.ErrorList.Add("dm_name_show_temp不在业务域中！");
                }
                else
                {
                    //判定Input中是否存在规则中必须要存在的值
                    Dictionary<string, string> dic = InputAttrRules(inputAttr, inputAttrList);
                    if (dic == null || dic.Count == 0) return;
                    foreach (var item in dic)
                    {
                        result.ErrorList.Add(item.Value);
                    }
                }
            }
        }

        private  void Detection(WorkflowProcessModule ProcessInfo, CheckResult result)
        {
           
            if (string.IsNullOrEmpty(ProcessInfo.DomainXml))
            {
                return;
            }
            else if (string.IsNullOrEmpty(ProcessInfo.MainTable))
            {
                return;
            }
            else if (!string.IsNullOrEmpty(ProcessInfo.MainTable) && string.IsNullOrEmpty(ProcessInfo.MainTableXml))
            {
                result.ErrorList.Add("对应业务对象不存在！");
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(ProcessInfo.DomainXml);
            XmlDocument xmlMain = new XmlDocument();
            xmlMain.LoadXml(ProcessInfo.MainTableXml);

            Verification(xmlDoc, xmlMain, result, "/BusinessType/Item/Domain");
            Verification(xmlDoc, xmlMain, result, "/BusinessType/Item/Group/Item/Domain");

        }

        private  void Verification(XmlDocument xmlDoc, XmlDocument xmlMain, CheckResult result, string path)
        {
            foreach (XmlNode node in xmlDoc.SelectNodes(path))
            {
                if (node.Attributes["isuser"].Value == "0" && xmlDoc.SelectNodes(path + "[@name='" + node.Attributes["name"].Value + "']").Count == 0)
                {

                    result.ErrorList.Add(node.Attributes["name"].Value + "在对应的业务对象中不存在此节点！");
                }
            }
        }

        /// <summary>
        /// html中内容进行检测
        /// </summary>
        /// <param name="ProcessInfo"></param>
        /// <param name="obj"></param>
        public  void RegularExpressionRulesList(WorkflowProcessModule ProcessInfo, CheckResult result)
        {
            string htmlContent = ProcessInfo.htmlContent;
           
            if (string.IsNullOrEmpty(htmlContent)) return;

            foreach (rulesitem rulesitem in GetHtmlContentConfig().rulesitems)
            {
                if (System.Text.RegularExpressions.Regex.Matches(htmlContent, rulesitem.rule, System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count > 0)
                {
                    result.ErrorList.Add("html内容异常");
                }
            }
        }


        private HtmlRulesConfig _mHtmlRulesConfig;
        private HtmlRulesConfig mHtmlRulesConfig
        {
            get
            {
                if (_mHtmlRulesConfig != null) return _mHtmlRulesConfig;

                string path = @"Config\HtmlRulesConfig.config";
                string configPath = Utility.GetBaseDirectory() + path;

                if (!File.Exists(configPath))
                {
                    return null;
                }
                HtmlRulesConfig config;
                using (var fs = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var xsl = new XmlSerializer(typeof(HtmlRulesConfig));
                    config = (HtmlRulesConfig)xsl.Deserialize(fs);
                }


                return config;
            }
        }

    
        private HtmlContentConfig GetHtmlContentConfig()
        {
            string htmlRulesString = @"Config\HtmlContentConfig.config";
            string configPath = Utility.GetBaseDirectory() + htmlRulesString;

            if (!File.Exists(configPath))
            {
                return null;
            }
            HtmlContentConfig config;
            using (var fs = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var xsl = new XmlSerializer(typeof(HtmlContentConfig));
                config = (HtmlContentConfig)xsl.Deserialize(fs);
            }
          

            return config;
        }

        public static Dictionary<string, object> GetInputInfo(HtmlNode input, CheckResult result)
        {
            Dictionary<string, object> domain = new Dictionary<string, object>();
            //对于属性为value的，暂时不插入
            input.Attributes.ToList().ForEach(attr =>
            {
                if (!attr.Name.Equals("value", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        domain.Add(attr.Name, attr.Value);
                    }
                    catch (Exception ex)
                    {

                        result.ErrorList.Add("input标签有重复的" + attr.Name + "属性");
                    }
                }
            });
            ///获取dm_name值
            if (!domain.ContainsKey("dm_name"))
            {
                if (domain.ContainsKey("name"))
                {
                    domain.Add("dm_name", domain["name"]);
                }
            }
            ///获取dm_namemobile值
            if (!domain.ContainsKey("dm_namemobile"))
            {
                if (domain.ContainsKey("dm_name"))
                {
                    domain.Add("dm_namemobile", domain["dm_name"]);
                }
            }

            if (!domain.ContainsKey("dm_displaytype"))
            {
                if (domain.ContainsKey("dm_type"))
                {
                    domain.Add("dm_displaytype", domain["dm_type"]);
                }
            }
            else
            {
                if (domain["dm_displaytype"].Equals("") && domain.ContainsKey("dm_type"))
                {
                    domain.Add("dm_displaytype", domain["dm_type"]);
                }
            }

            //展示用的
            //展示用的
            if (!domain.ContainsKey("dm_name"))
            {
                domain.Add("dm_name_show_temp", "");
            }
            else
            {
                domain.Add("dm_name_show_temp", domain["dm_name"]);
            }

            if (!domain.ContainsKey("dm_namemobile"))
            {
                domain.Add("dm_namemobile_show_temp", "");
            }
            else
            {
                domain.Add("dm_namemobile_show_temp", domain["dm_namemobile"]);
            }

            if (!domain.ContainsKey("dm_displaytype"))
            {
                domain.Add("dm_displaytype_show_temp", "");
            }
            else
            {
                domain.Add("dm_displaytype_show_temp", domain["dm_displaytype"]);
            }
            return domain;
        }

        /// <summary>
        /// Input中是否存在规则中必须要存在的值（数据类型是否支持，数据类型对应的属性是否存在）
        /// </summary>
        /// <param name="inputAttr">input中的属性</param>
        /// <returns></returns>
        private  Dictionary<string, string> InputAttrRules(Dictionary<string, object> inputAttr, List<Dictionary<string, object>> inputAttrList)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (mHtmlRulesConfig == null) return dic;
            var items = getItemList(inputAttr, dic);
            foreach (Item item in items)
            {
                if (item.keys == null)
                {
                    return dic;
                }
                foreach (keyInfo keyInfo in item.keys)
                {
                    if (keyInfo.checktype == "all")
                    {
                        //所有类型都进行比较
                    }
                    else if (keyInfo.checktype == "edit")
                    {
                        //编辑类型都进行比较
                        if (ProcessInfo.EditBusinessDomain == null || ProcessInfo.EditBusinessDomain.Count == 0 || !ProcessInfo.EditBusinessDomain.ContainsKey(inputAttr["dm_name_show_temp"].ToString()))
                        {
                            continue;
                        }
                    }
                    else if (keyInfo.checktype == "readonly")
                    {
                        //不编辑类型都进行比较
                        if (ProcessInfo.EditBusinessDomain.Count != 0 && ProcessInfo.EditBusinessDomain.ContainsKey(inputAttr["dm_name_show_temp"].ToString()))
                        {
                            continue;
                        }
                    }

                    AttrKeyInfo(keyInfo, inputAttr, inputAttrList, dic);
                }
            }
            return dic;
        }

        /// <summary>
        /// 获取inputAttr对应配置属性值
        /// </summary>
        /// <param name="inputAttr"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private  Item[] getItemList(Dictionary<string, object> inputAttr, Dictionary<string, string> dic)
        {
            //验证Input类型只是支持
            var items = (from item in mHtmlRulesConfig.attrRules where item.type.ToLower() == inputAttr["dm_displaytype_show_temp"].ToString().ToLower() select item).ToArray();
            if (items.Count() == 0)
            {

                addDic(dic, "error", inputAttr["dm_namemobile_show_temp"].ToString() + "的“" + inputAttr["dm_displaytype_show_temp"].ToString() + "”数据类型暂不支持展示！");
            }
            else
            {
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.tip))
                    {
                        addDic(dic, "", getTip(inputAttr, item.tip));
                    }
                }
            }
            return items;
        }

        /// <summary>
        /// 添加dic
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="dic"></param>
        private static void addDic(Dictionary<string, string> dic, string key, string val)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] += val;
            }
            else
            {
                dic.Add(key, val);
            }
        }

        /// <summary>
        /// 获取语句
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="tip"></param>
        /// <returns></returns>
        private static string getTip(Dictionary<string, object> dic, string tip)
        {
            foreach (string key in System.Text.RegularExpressions.Regex.Matches(tip, "(?<={)[^{}]*?(?=})", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                if (dic[key] != null)
                {
                    tip = tip.Replace("{" + key + "}", dic[key].ToString());
                }
            }
            return tip;
        }

        private static void AttrKeyInfo(keyInfo keyInfo, Dictionary<string, object> inputAttr, List<Dictionary<string, object>> inputAttrList, Dictionary<string, string> dic)
        {



            if (!inputAttr.ContainsKey(keyInfo.key))
            {
                if (!string.IsNullOrEmpty(keyInfo.tip))
                {
                    addDic(dic, "error", getTip(inputAttr, keyInfo.tip));
                }
                else
                {
                    addDic(dic, "error", inputAttr["dm_namemobile_show_temp"].ToString() + "的“" + inputAttr["dm_displaytype_show_temp"].ToString() + "”数据类型缺少“" + keyInfo.key + "”属性！");
                }
            }
            else if (inputAttr.ContainsKey(keyInfo.key) && keyInfo.notnull == "1" && string.IsNullOrEmpty(inputAttr[keyInfo.key].ToString()))
            {
                addDic(dic, "error", inputAttr["dm_namemobile_show_temp"].ToString() + "的“" + inputAttr["dm_displaytype_show_temp"].ToString() + "”数据类型“" + keyInfo.key + "”属性值不能为空！");
            }
            else if (inputAttr.ContainsKey(keyInfo.key) && !string.IsNullOrEmpty(keyInfo.KeyHtml))
            {
                if (string.IsNullOrEmpty(keyInfo.Rules))
                {
                    keyInfo.Rules = ".*";
                }
                var keyRules = keyInfo.Rules.Split('|');
                foreach (string keyRule in keyRules)
                {
                    foreach (var str in System.Text.RegularExpressions.Regex.Matches(inputAttr[keyInfo.key].ToString(), keyInfo.Rules, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        if (string.IsNullOrEmpty(str.ToString()))
                        {
                            continue;
                        }
                        Boolean isTrue = false;
                        foreach (var iattr in inputAttrList)
                        {
                            if (iattr[keyInfo.KeyHtml].ToString() == str.ToString())
                            {
                                isTrue = true;
                                break;
                            }
                        }
                        if (!isTrue)
                        {
                            addDic(dic, "error", inputAttr["dm_namemobile_show_temp"].ToString() + "的“" + inputAttr["dm_displaytype_show_temp"].ToString() + "”数据类型“" + keyInfo.key + "”属性中参与计算的对象找不到！");
                        }
                    }
                }

            }
        }
    }

    public class WorkflowProcessModule
    {
        public int Num { get; set; }
        /// <summary>
        /// 流程GUID
        /// </summary>
        public string ProcessGUID { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        public string ProcessName { get; set; }

        public string ProcessKindName { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessTypeGUID { get; set; }

        /// <summary>
        /// 子系统
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Sheet:HTML表单
        /// Common：Word表单
        /// Extend：扩展表单
        /// Excel：Excel表单
        /// </summary>
        public int DocumentType { get; set; }

        public string DocumentTypeName { get; set; }

        public int DocumentTypeInt { get; set; }

        /// <summary>
        /// Default：默认模式
        /// SingleLine:单列模式
        /// Custom:表格模式
        /// URL:定制模式
        /// </summary>
        public string FormMode { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string PhoneFormMode { get; set; }

        public string SingleFormDomainXML { get; set; }

        public string PhoneUrl { get; set; }

        public string DomainXml { get; set; }

        public string PhoneDocumentContent { get; set; }

        /// <summary>
        /// 编辑列
        /// </summary>
        public Dictionary<string, string> EditBusinessDomain { get; set; }

        /// <summary>
        /// html内容
        /// </summary>
        public string htmlContent { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public int IsActive { get; set; }

        public string IsActiveName { get; set; }

        /// <summary>
        /// 文档目录
        /// </summary>
        public string DocumentPath { get; set; }

        public string MainTable { get; set; }
        /// <summary>
        /// 业务类型xml
        /// </summary>
        public string MainTableXml { get; set; }
    }

    public class HtmlContentConfig
    {
        public rulesitem[] rulesitems;
    }

    public class rulesitem
    {
        /// <summary>
        /// 规则
        /// </summary>
        public string rule { get; set; }
        public string title { get; set; }

        private string _errorlevel;
        public string errorlevel
        {
            get { return _errorlevel; }
            set
            {
                if (string.IsNullOrEmpty(value)) { value = "error"; }
                _errorlevel = value;
            }
        }
    }




    public class HtmlRulesConfig
    {
        [XmlArrayItem("Item")]
        public List<Item> attrRules = new List<Item>();
    }
    [Serializable]
    public class Item
    {
        /// <summary>
        /// 类型
        /// </summary>
        [XmlAttribute]
        public string type { get; set; }
        [XmlAttribute]
        public string remarks { get; set; }
        [XmlAttribute]
        public string tip { get; set; }


        private string _errorlevel;
        [XmlAttribute]
        public string errorlevel
        {
            get { return _errorlevel; }
            set
            {
                if (string.IsNullOrEmpty(value)) { value = "error"; }
                _errorlevel = value;
            }
        }
        [XmlArrayItem("keyInfo")]
        public List<keyInfo> keys = new List<keyInfo>();
    }
    [Serializable]
    public class keyInfo
    {
        /// <summary>
        /// 键值
        /// </summary>
        [XmlAttribute]
        public string key { get; set; }
        /// <summary>
        /// 是否验证值不能为空
        /// </summary>
        [XmlAttribute]
        public string notnull { get; set; }

        /// <summary>
        /// html对应属性
        /// </summary>
        [XmlAttribute]
        public string KeyHtml { get; set; }
        /// <summary>
        /// 解析对应的值
        /// </summary>
        [XmlAttribute]
        public string Rules { get; set; }

        private string _checktype;
        /// <summary>
        /// all 所有检查 edit 编辑检查 readonly 非编辑检查
        /// </summary>
        [XmlAttribute]
        public string checktype
        {
            get { return _checktype; }
            set
            {
                if (string.IsNullOrEmpty(value)) { value = "all"; }
                _checktype = value;
            }
        }

        /// <summary>
        /// 提示语句
        /// </summary>
        [XmlAttribute]
        public string tip { get; set; }

        private string _errorlevel;
        /// <summary>
        /// 错误级别
        /// </summary>
        [XmlAttribute]
        public string errorlevel
        {
            get { return _errorlevel; }
            set
            {
                if (string.IsNullOrEmpty(value)) { value = "error"; }
                _errorlevel = value;
            }
        }

    }
}
