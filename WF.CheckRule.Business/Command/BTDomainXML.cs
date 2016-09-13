using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace WF.CheckRule.Business.Form
{
    public class BTDomainXML
    {
        public string EnsureHtmlFormFieldsAllEnable(string HTMLContent,string BTDomainXML)
        {
            List<string> htmlFields = GetHtmlFormDomainFields(HTMLContent);

            List<string> AllDomainFields = GetAllDomainFields(BTDomainXML);

            var field = htmlFields.Where(n => AllDomainFields.Contains(n) == false).FirstOrDefault();

            if (field != null)
            {
                return field;
            }
            

            return string.Empty;
        }

        protected List<string> GetHtmlFormDomainFields(string HTMLContent)
        {
            List<string> fieldList = new List<string>();
            if (String.IsNullOrEmpty(HTMLContent)) {
                return fieldList;
            }

            Regex regexDomains = new Regex(@"<input.*?\bdm_name\b\s*=\s*""?(.+?)[""?|\s]", RegexOptions.IgnoreCase);
            MatchCollection mcDomains = regexDomains.Matches(DeleteComments(HTMLContent));
            for (int i = 0; i < mcDomains.Count - 1; i++) {
                fieldList.Add(mcDomains[i].Groups[1].ToString());
            }
            return fieldList;
        }

        protected List<string> GetAllDomainFields(string BTDomainXML)
        {
            List<string> _allDomainFields = new List<string>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(BTDomainXML);
            XmlNodeList nodeList = xmlDoc.SelectNodes("//Domain");
            foreach (XmlNode xn in nodeList) {
                _allDomainFields.Add(xn.Attributes["name"].Value.ToString());
                //_allDomainFields.Add(xn.Attributes("name").value);
            }
            //  _allDomainFields = (From node As XmlNode In nodeList Select node.Attributes("name").Value).ToList();
            //_allDomainFields = from node  in  nodeList
            //                   select node.Attributes("name").Value;
            //_allDomainFields = nodeList.AsQueryable().Select(n => n.Attributes("name").Value).ToList();
           


            return _allDomainFields;

        }

        private string DeleteComments(string htmContent)
        {
            int commentStartIndex = htmContent.IndexOf("<!--", 0, StringComparison.Ordinal);
            if (commentStartIndex == -1) {
                return htmContent;
            }
            int commentEndIndex = htmContent.IndexOf("-->", commentStartIndex, StringComparison.Ordinal);
            if (commentEndIndex == -1) {
                return htmContent;
            }
            return DeleteComments(htmContent.Substring(0, commentStartIndex) + htmContent.Substring(commentEndIndex + 3));
        }
    }
}
