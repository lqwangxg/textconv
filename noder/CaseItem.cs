using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Text.Common;


namespace noder
{
    public class CaseItem
    {
        public string title;
        public string subpath;
        public string eventKey;
        public string eventName;
        public string eventText;
        public string caseDesc;
        public string condition;

        private string attrname;
        private static string ptwords = @"\>([\w\s]+)";

        public void refresh(XPathRuleItem ruleItem, HtmlNode node)
        {
            Console.WriteLine("ruleItem:"+ruleItem.name);
                    
            setEventKeyByAttr(ruleItem, node);
            Console.WriteLine("eventKey:"+this.eventKey);
            setEventNameByAttr(ruleItem, node);
            
            if (string.IsNullOrEmpty(this.eventName) || !Regex.IsMatch(eventName, @"\w+"))
            {
                if (!string.IsNullOrEmpty(this.eventKey))
                {
                    if (ruleItem.wordMap.ContainsKey(this.eventKey))
                    {
                        this.eventName = ruleItem.wordMap[this.eventKey];
                    }
                    else
                    {
                        this.eventName = this.eventKey;
                    }
                }
            }
            else if (Regex.IsMatch(this.eventName, ptwords))
            {
                this.eventName = StringUtils.GetMatchGroup(this.eventName, ptwords, 1);
            }
            if (string.IsNullOrEmpty(this.eventName))
            {
                if(node.FirstChild != null)
                {
                    if (node.FirstChild.Name.Contains("#text"))
                    {
                        this.eventName = node.FirstChild.InnerText;
                    }
                }
            } 
            else if (ruleItem.wordMap.ContainsKey(this.eventName))
            {
                this.eventName = ruleItem.wordMap[this.eventName];
            }
            if (string.IsNullOrEmpty(this.eventName))
            {
                Console.WriteLine("eventName:" + this.eventName);
                return;
            }
            else if(Regex.IsMatch(this.eventName, @"\n|\r"))
            {
                this.eventName = Regex.Replace(this.eventName, @"\n|\r", "");
            }
            this.eventName = this.eventName.Trim();
            Console.WriteLine("eventName:" + this.eventName);
        }

        private string ToEventText(XPathRuleItem ruleItem)
        {
            if (string.IsNullOrEmpty(this.eventName)) return string.Empty;

            foreach (var kv in ruleItem.namePatternFormats)
            {
                if (string.IsNullOrEmpty(kv.textformat)) continue;
                if (Regex.IsMatch(eventName, kv.pattern, RegexOptions.IgnoreCase))
                {
                    string format = kv.textformat;
                    if (Regex.IsMatch(format, @"\$\d+"))
                    {
                        format = Regex.Replace(eventName, kv.pattern, kv.textformat);
                    }
                    if (format.Contains("eventname"))
                    {
                        return StringUtils.ReplaceKeyValue(kv.textformat, "eventname", eventName);
                    }
                    else
                    {
                        return format;
                    }
                }
            }
            return string.Format(ruleItem.eventText, eventName);
        }
        private string ToCaseDesc(XPathRuleItem ruleItem)
        {
            if (string.IsNullOrEmpty(this.eventName)) return string.Empty;

            foreach (var kv in ruleItem.namePatternFormats)
            {
                if (string.IsNullOrEmpty(kv.caseformat)) continue;
                if (Regex.IsMatch(eventName, kv.pattern, RegexOptions.IgnoreCase))
                {
                    string format = kv.caseformat;
                    if(Regex.IsMatch(format, @"\$\d+"))
                    {
                        format = Regex.Replace(eventName, kv.pattern, kv.caseformat);
                    }
                    if (format.Contains("eventname"))
                    {
                        return StringUtils.ReplaceKeyValue(kv.caseformat, "eventname", eventName);
                    }
                    else
                    {
                        return format;
                    }
                }
            }
            return StringUtils.ReplaceKeyValue(ruleItem.caseDescFormat, "eventname", eventName);
        }
        public void ToCaseDesc(XPathRuleItem ruleItem, HtmlNode node)
        {
            if (ruleItem.name.Contains("sortlink"))
            {
                if (node.OuterHtml.Contains("ASC"))
                {
                    eventText = string.Format(ruleItem.eventText, eventName, "△");
                    caseDesc = ToCaseDesc(ruleItem);
                    caseDesc = StringUtils.ReplaceKeyValue(caseDesc, "sorttype", "昇順");
                }
                else
                {
                    eventText = string.Format(ruleItem.eventText, eventName, "▽");
                    caseDesc = ToCaseDesc(ruleItem);
                    caseDesc = StringUtils.ReplaceKeyValue(caseDesc, "sorttype", "降順");
                }
                return;
            }
            
            if (ruleItem.name.Contains("calender"))
            {
                if (eventKey.EndsWith("_S"))
                {
                    eventName = eventName + " 開始";
                }
                else if(eventKey.EndsWith("_E"))
                {
                    eventName = eventName + " 終了";
                }
                this.eventText = ToEventText(ruleItem);
                this.caseDesc = ToCaseDesc(ruleItem);
                return;
            }
            
            if (ruleItem.name.Contains("select")
             || ruleItem.name.Contains("tabpage"))
            {
                //初期表示：DropList
                this.eventText = ToEventText(ruleItem);
                this.caseDesc = ToCaseDesc(ruleItem);
                return;
            }

            this.eventText = ToEventText(ruleItem);
            if (!string.IsNullOrEmpty(this.eventName) && ruleItem.caseMap.ContainsKey(this.eventName))
            {
                this.caseDesc = ruleItem.caseMap[this.eventName];
            }else{
                this.caseDesc = ToCaseDesc(ruleItem);
            }
        }

        private void setEventKeyByAttr(XPathRuleItem rule, HtmlNode node)
        {
            if (!string.IsNullOrEmpty(eventKey)) return;
            
            foreach (var k in rule.keypattern)
            {
                if (!node.Attributes.Contains(k.attrname)) continue;
                attrname = k.attrname;
                string attrValue = node.Attributes[k.attrname].Value;
                if (Regex.IsMatch(attrValue, @"^[\w\.]+$"))
                {
                    eventKey = attrValue;
                }
                else if (Regex.IsMatch(attrValue, k.pattern))
                {
                    if (string.IsNullOrEmpty(k.replacement))
                    {
                        eventKey = attrValue;
                    }else
                    {
                        eventKey = Regex.Replace(attrValue, k.pattern, k.replacement);
                    }
                }
                
                if (!string.IsNullOrEmpty(eventKey)) return;
            }
        }
        private void setEventNameByAttr(XPathRuleItem rule, HtmlNode node)
        {
            if (!string.IsNullOrEmpty(eventName)) return;

            HtmlDocument htmlDoc = node.OwnerDocument;
            foreach (var xpath in rule.textxpath)
            {
                // error case 
                if (string.IsNullOrEmpty(eventKey)) continue;
                
                string xpath2 = string.Format(xpath, attrname, eventKey);
                var n2 = htmlDoc.DocumentNode.SelectNodes(xpath2);
                if (n2 != null)
                {
                    HtmlNode firstNode = n2.First();
                    string innerText = firstNode.InnerText.Trim();
                    if (Regex.IsMatch(innerText,@"\w+"))
                    {
                        this.eventName = WebUtility.HtmlDecode(innerText);
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(eventName))
            {
                this.eventName = this.eventKey;
            }
        }

        public string ToStringLine()
        {
            List<string> lstItem = new List<string>();
            lstItem.Add(title);
            lstItem.Add(subpath);
            //lstItem.Add(eventKey);
            //lstItem.Add(eventName);
            lstItem.Add(eventText);
            lstItem.Add(caseDesc);
            lstItem.Add(condition);
            return string.Join("\t", lstItem);
        }
    }
}
