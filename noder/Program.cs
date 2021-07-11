using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Text.Common;

namespace noder
{
    class Program
    {
        private static string resultFolder = string.Empty;
        private static string cmdType = string.Empty;
        static void Main(string[] args)
        {
            //args check 
            //==============================================================
            if (args.Length < 4)
            {
                Console.WriteLine("noder -c [case|element]  [-d dirpath]  [-f filepath] ");
                return;
            }
            cmdType = StringUtils.GetArgValue("-c", args);
            //==============================================================
            if (string.IsNullOrEmpty(cmdType))
            {
                Console.WriteLine("noder -c [case|element]  [-d dirpath]  [-f filepath] ");
                return;
            }

            string srcfolder = string.Empty;
            if (args.Contains("-d"))
            {
                srcfolder = StringUtils.GetArgValue("-d", args);
                if (string.IsNullOrEmpty(srcfolder))
                {
                    srcfolder = Config.GetAppSettingValue("srcfolder");
                }
                resultFolder = StringUtils.GetMatchGroup(srcfolder, @"(\w+)\\*$", 1);
            }
            string srcFile = string.Empty;
            if (args.Contains("-f"))
            {
                srcFile = StringUtils.GetArgValue("-f", args);
            }

            if (string.IsNullOrEmpty(srcfolder) && string.IsNullOrEmpty(srcFile))
            {
                Console.WriteLine("noder -c [case|element]  [-d dirpath]  [-f filepath] ");
                return;
            }

            List<XPathRuleItem> rules = new List<XPathRuleItem>();
            string ruleFolderPath = Config.GetAppSettingValue("xpath.rule.yml");
            YmlLoader.Load(rules, ruleFolderPath);
            HtmlParseFolder(srcfolder, rules);
            HtmlParseFile(srcFile, rules);

        }

        #region Html Parser for export
        private static void HtmlParseFolder(string folder, List<XPathRuleItem> ruleItems)
        {
            if (!Directory.Exists(folder)) return;

            string ext = Config.GetAppSettingValue2("xpath.ext", ".(html?|jsp)$");
            foreach (string filePath in Directory.GetFiles(folder))
            {
                if (!Regex.IsMatch(filePath, ext)) continue;
                HtmlParseFile(filePath, ruleItems);
            }
            foreach (string filePath in Directory.GetDirectories(folder))
            {
                HtmlParseFolder(filePath, ruleItems);
            }
        }

        private static void HtmlParseFile(string filePath, List<XPathRuleItem> ruleItems)
        {
            if (!File.Exists(filePath)) return;
            if (cmdType.Equals("case"))
            {
                try
                {
                    CaseFile cf = new CaseFile(filePath);
                    cf.Parse(ruleItems);
                    cf.Export(resultFolder);
                    Console.WriteLine(string.Format("{0}\t{1}", cf.titleText, cf.SourcePath));
                    foreach (var msg in cf.errmsgs)
                    {
                        Console.WriteLine(msg);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.Source);
                    Console.Write(ex.StackTrace);
                }
            }
            if (cmdType.Equals("element"))
            {
                try
                {
                    CaseFile cf = new CaseFile(filePath);
                    cf.Parse(ruleItems);
                    cf.Export(resultFolder);
                    Console.WriteLine(string.Format("{0}\t{1}", cf.titleText, cf.SourcePath));
                    foreach (var msg in cf.errmsgs)
                    {
                        Console.WriteLine(msg);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.Source);
                    Console.Write(ex.StackTrace);
                }
            }
        }

        #endregion
    }
}
