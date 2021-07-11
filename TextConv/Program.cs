using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Text.Common;
using Text.Web;
namespace TextConv
{
    public class Program
    {
        private static string resultFolder = string.Empty;
        static void Main(string[] args)
        {
            //args check 
            //==============================================================
            if (args.Length < 2)
            {
                Console.WriteLine("TextConv [-p PATTERN] [-r REPLACEMENT] [-d srcfolder] [-f srcfile]");
                Console.WriteLine("TextConv [-c COMMAND_KEY] [-x XPATH] [-d srcfolder] [-f srcfile]");
                return;
            }
            string cmd = StringUtils.GetArgValue("-c", args);

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
                Console.WriteLine("App.config setting srcfolder is required.");
                return;
            }
            //==============================================================
            if (args.Contains("-web"))
            {
                WebRunner wr = new WebRunner();
                if (!string.IsNullOrEmpty(srcFile))
                {
                    wr.Run(srcFile);
                }
                else if (!string.IsNullOrEmpty(srcfolder))
                {
                    wr.RunBatch(srcfolder);
                }
            }
            //==============================================================
            if (args.Contains("-x")) 
            {
                List<XPathRuleItem> rules = new List<XPathRuleItem>();
                string ruleFolderPath = Config.GetAppSettingValue("xpath.rule.yml");
                YmlLoader.Load(rules, ruleFolderPath, cmd);
                HtmlParseFolder(srcfolder, rules);
                HtmlParseFile(srcFile, rules);
            }
            
            //==============================================================
            if (args.Contains("-c"))
            {
                List<ReplaceRule> repRules = new List<ReplaceRule>();
                string ruleFolderPath = Config.GetAppSettingValue("replace.rule.yml");
                YmlLoader.Load(repRules, ruleFolderPath, cmd);

                if (args.Contains("-p"))
                {
                    ReplaceRuleItem ri = new ReplaceRuleItem();
                    ri.pattern = StringUtils.GetArgValue("-p", args);
                    ri.replacement = StringUtils.GetArgValue("-r", args);
                    if (!string.IsNullOrEmpty(ri.pattern))
                    {
                        string content = StringUtils.GetArgValue("-input", args);
                        if (!string.IsNullOrEmpty(content))
                        {
                            Console.WriteLine(ri.replaceText(content));
                        }
                        else
                        {
                            ReplaceRule rule = new ReplaceRule();
                            rule.rules.Add(ri);
                            repRules.Add(rule);
                        }
                    }
                }
                
                ReplaceFolder(srcfolder, repRules);
                ReplaceFile(srcFile, repRules);
            }
            //==============================================================
            if (args.Contains("-rename"))
            {
                if (args.Contains("-p") && args.Contains("-r"))
                {
                    ReplaceRuleItem ri = new ReplaceRuleItem();
                    ri.pattern = StringUtils.GetArgValue("-p", args);
                    ri.replacement = StringUtils.GetArgValue("-r", args);
                    ri.Rename(srcfolder);
                }
            }
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

            try
            {
                CaseFile cf = new CaseFile(filePath);
                cf.Parse(ruleItems);
                cf.Export(resultFolder);
                Console.WriteLine(string.Format("{0}\t{1}",cf.titleText, cf.SourcePath));
                foreach (var msg in cf.errmsgs)
                {
                    Console.WriteLine(msg);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Source);
                Console.Write(ex.StackTrace);
            }
        }

        private static void ReplaceFolder(string folder, List<ReplaceRule> ruleItems)
        {
            if (!Directory.Exists(folder)) return;
            foreach (ReplaceRule rule in ruleItems)
            {
                rule.ReplaceFolder(folder);
            }
        }
        private static void ReplaceFile(string filePath, List<ReplaceRule> ruleItems)
        {
            if (!File.Exists(filePath)) return;
            foreach (ReplaceRule rule in ruleItems)
            {
                rule.ReplaceFile(filePath);
            }
        }

        #endregion

        
    }
}
