using System;
using System.Collections.Generic;
using System.IO;

namespace noder
{
    public class XPathKeyPattern
    {
        public string attrname;
        public string pattern;
        public string replacement;
    }
    public class PatternFormat
    {
        public string pattern;
        public string textformat = string.Empty;
        public string caseformat = string.Empty;
    }
    public class XPathRuleItem
    {
        public string name;
        public string xpath;
        public List<XPathKeyPattern> keypattern = new List<XPathKeyPattern>();
        public List<string> textxpath = new List<string>();
        public List<PatternFormat> namePatternFormats = new List<PatternFormat>();
        public string eventText;
        public string caseDescFormat;
        
        public Dictionary<string, string> caseMap = new Dictionary<string, string>();
        public Dictionary<string, string> wordMap = new Dictionary<string, string>();
        public XPathRuleItem()
        {
            //LoadMap(Config.GetAppSettingValue("casemapFile"), caseMap);
            //LoadMap(Config.GetAppSettingValue("wordmapfile"), wordMap);
        }
        //private void LoadMap(string filename, Dictionary<string, string> map)
        //{
        //    if (File.Exists(filename))
        //    {
        //        FileHelper.FillFromFile(filename, map);
        //    }
        //}
    }
}
