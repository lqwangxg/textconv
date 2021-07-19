using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Text.Common;

namespace TextConv
{
    public class ReplaceRule
    {
        public string name = string.Empty;
        public List<ReplaceRuleItem> rules = new List<ReplaceRuleItem>();

        public string filefilter = string.Empty;
        public bool fileSkip = false;
        public bool commentRequired = false;

        public string aftercheckpattern = string.Empty;

        private List<string> Results = new List<string>();
        private bool isSkipFile(string filepath)
        {
            if (string.IsNullOrEmpty(filefilter)) return false;

            if (Regex.IsMatch(filepath, filefilter, RegexOptions.IgnoreCase))
            {
                return this.fileSkip;
            }
            else
            {
                return !this.fileSkip;
            }
        }
        public CommentConfigItem cconfig { get; private set; }
        public void Init()
        {
            foreach (ReplaceRuleItem item in rules)
            {
                item.parent = this;
            }
        }
        public static CommentConfigItem GetCommentConfig(string ext)
        {
            string ympPath = "CommentConfig.yml";
            CommentConfig config = YmlLoader.LoadFromFile<CommentConfig>(ympPath);
            if (config == null)
            {
                Console.WriteLine("Error:...Can't found ReplaceConfig.yml...");
                return null;
            }

            foreach (CommentConfigItem item in config.rules)
            {
                if (item.fileExtension.Contains(ext))
                {
                    return item;
                }
            }

            return null;
        }
        public string ReplaceText(string content)
        {
            if(cconfig == null)
            {
                cconfig = GetCommentConfig(".default");
            }

            Results.Clear();
            foreach (var rule in rules)
            {
                content = rule.replaceText(content);
                Results.AddRange(rule.Results());
            }

            if (!string.IsNullOrEmpty(aftercheckpattern))
            {
                MatchCollection ms = Regex.Matches(content, aftercheckpattern);
                foreach (Match m in ms)
                {
                    Console.WriteLine("\t{0}\t{1}", aftercheckpattern, m.Value);
                }
            }
            return content;

        }
        public void ReplaceFile(string file)
        {
            //ファイル単位置換
            Results.Clear();
            if (!File.Exists(file)) return;
            if (isSkipFile(file)) return;
            string ext = new FileInfo(file).Extension;
            cconfig = GetCommentConfig(ext);

            string content = File.ReadAllText(file, Config.Encoding);
            content = ReplaceText(content);
            if (Results.Count > 0)
            {
                File.WriteAllText(file, content, Config.Encoding);
                Console.WriteLine("{0}\n{1}", file, string.Join("\n", Results));
            }
        }

        public void ReplaceFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;

            DirectoryInfo di = new DirectoryInfo(folderPath);
            foreach (FileInfo file in di.GetFiles())
            {
                ReplaceFile(file.FullName);
            }

            foreach (var sdi in di.GetDirectories())
            {
                ReplaceFolder(sdi.FullName);
            }
        }

    }
}
