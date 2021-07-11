using HtmlAgilityPack;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using Text.Common;

namespace noder
{
    public class CaseFile
    {
        private string title;
        public string titleText { get; private set; }
        public string FileName { get; private set; }
        public string ext { get; private set; }
        public string subFileName { get; private set; }
        public List<CaseItem> listNode = new List<CaseItem>();
        public List<string> errmsgs = new List<string>();

        public string exportFile { get; private set; }
        public string SourcePath { get; private set; }
        private HtmlDocument htmlDoc;
        private static string subFilePathSetting = Config.GetAppSettingValue("subHtmlPath");
        public CaseFile(string htmlPath)
        {
            this.htmlDoc = new HtmlDocument();
            this.SourcePath = htmlPath;
            this.htmlDoc.Load(File.OpenRead(htmlPath), Config.Encoding);
            this.subFileName = StringUtils.GetMatchGroup(htmlPath, subFilePathSetting, 1);

            FileInfo fi = new FileInfo(htmlPath);
            this.FileName = StringUtils.GetMatchGroup(fi.Name, @"(.+)\.\w+$", 1);
            this.ext = fi.Extension;
            GetTitle();
        }

        private void GetTitle()
        {
            if (string.IsNullOrEmpty(title))
            {
                var ns = htmlDoc.DocumentNode.SelectNodes("//title");
                if(ns != null)
                {
                    title = ns.First().InnerText;
                    title = WebUtility.HtmlDecode(title);
                    if(Regex.IsMatch(title, @"\>(\w+)"))
                    {
                        title = StringUtils.GetMatchGroup(title, @"\>(\w+)", 1);
                    }
                }
            }
            titleText = title;

            if (!string.IsNullOrEmpty(titleText))
            {
                titleText = Regex.Replace(titleText, @"\$\{[\w\.\s]+\}", "unknown");
                string invalidchars = Config.GetAppSettingValue("xpath.invalid.chars");
                if (!string.IsNullOrEmpty(invalidchars))
                {
                    titleText = Regex.Replace(titleText, invalidchars, string.Empty);
                }
            }
        }
        
        public void Parse(List<XPathRuleItem> ruleItems)
        {
            listNode.Clear();

            CaseItem ci = new CaseItem();
            listNode.Add(ci);
            ci.title = this.titleText;
            ci.subpath = this.subFileName;
            ci.eventText = "初期表示";
            ci.caseDesc = "画面が表示される";

            bool writeLog = bool.Parse(Config.GetAppSettingValue("writeLog"));
            foreach (var item in ruleItems)
            {
                var ns = htmlDoc.DocumentNode.SelectNodes(item.xpath);
                if (ns == null) continue;
                foreach (var node in ns)
                {
                    ci = new CaseItem();
                    ci.title = this.titleText;
                    ci.subpath = this.subFileName;

                    ci.refresh(item, node);
                    ci.ToCaseDesc(item, node);
                    if (string.IsNullOrEmpty(ci.eventName))
                    {
                        errmsgs.Add(string.Format(" ★Error★:   rule:{4}={5} eventkey={0}    eventText={1}  caseDesc={2}    nodehtml={3}",
                            ci.eventKey, ci.eventText, ci.caseDesc, node.OuterHtml, item.name, item.xpath));
                        ci.eventText = item.xpath;
                        if (string.IsNullOrEmpty(ci.caseDesc))
                        {
                            ci.caseDesc = node.OuterHtml;
                        }
                    }
                    else if (!Regex.IsMatch(ci.eventName, @"\w+"))
                    {
                        errmsgs.Add(string.Format(" ★Error★:   rule:{4}={5} eventkey={0}    eventName={6}    eventText={1}  caseDesc={2}    nodehtml={3}",
                            ci.eventKey, ci.eventText, ci.caseDesc, node.OuterHtml, item.name, item.xpath, ci.eventName));
                        if (string.IsNullOrEmpty(ci.caseDesc))
                        {
                            ci.caseDesc = node.OuterHtml;
                        }
                    }
                    ci.caseDesc = Regex.Replace(ci.caseDesc, "[\t\r\n]+", string.Empty);
                    if (writeLog)
                    {
                        errmsgs.Add(" ●INFO●:   " + ci.ToStringLine());
                    }
                    listNode.Add(ci);
                }
            }
        }

        public void Export(string resultFolder)
        {
            GetTitle();
            string caseFileName = Config.GetAppSettingValue("caseFileName");
            
            caseFileName = StringUtils.ReplaceKeyValue(caseFileName, "title", titleText);
            caseFileName = StringUtils.ReplaceKeyValue(caseFileName, "filename", this.FileName);
            caseFileName = StringUtils.ReplaceKeyValue(caseFileName, "ext", this.ext);

            string[] lines = listNode.Select(ci => ci.ToStringLine()).ToArray();
            string exportFolder = Config.GetAppSettingValue2("exportFolder", "exportText");
            if (!exportFolder.EndsWith("\\"))
            {
                exportFolder = exportFolder + "\\";
            }
            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
            }
            exportFolder = exportFolder + resultFolder+ "\\";
            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
            }

            exportFile = exportFolder + caseFileName;
            File.WriteAllLines(exportFile, lines);
        }
    }
}
