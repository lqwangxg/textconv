using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Text.Common
{
    public class StringUtils
    {

        public static string ReplaceMatchGroup(string input, string pattern, int groupIndex, string replacement)
        {
            Regex regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (regex.IsMatch(input))
            {
                return regex.Replace(input, new MatchEvaluator(
                    delegate (Match match)
                    {
                        return ReplaceCC(match, groupIndex, replacement);
                    }));
            }
            return input;
        }

        public static string ReplaceMatch(string input, string pattern, string replacement)
        {
            return ReplaceMatchGroup(input, pattern, 0, replacement);
        }

        public static string ReplaceKeyValue(string input, string key, string value)
        {
            string pattern = "\\{\\s*" + key + "\\s*\\}";
            input = ReplaceMatchGroup(input, pattern, 0, value);

            pattern = "\\[\\s*" + key + "\\s*\\]";
            input = ReplaceMatchGroup(input, pattern, 0, value);
            
            pattern = "(:\\s*" + key + ")\\W";
            input = ReplaceMatchGroup(input, pattern, 1, value);

            return input;
        }
        public static string GetMatchValue(string input, string pattern)
        {
            return GetMatchGroup(input, pattern, 0);
        }
        public static string GetPropertyValue(string line, string name)
        {
            return GetMatchGroup(line, string.Format(@"{0}\s*=[\s""]*([^""]+)[\s""]*", name), 1);
        }
        public static string GetNodeText(string line)
        {
            return GetMatchGroup(line, @"\>([^\<\>]+)\<", 1);
        }
        public static string GetMatchGroup(string input, string pattern, int groupIndex) 
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            Match m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (m.Success) 
            {
                if (groupIndex < m.Groups.Count) 
                {
                    return m.Groups[groupIndex].Value;
                }
            }
            return string.Empty; 
        }

        private static string ReplaceCC(Match m, int groupIndex, string replacement)
        {
            return m.Value.Replace(m.Groups[groupIndex].Value, replacement);
        }

        public static string DataTabletoString(DataTable dt)
        {
            string header = string.Join("|", dt.Columns.OfType<DataColumn>().Select(x => x.ColumnName));
            List<string> lstTable = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                List<string> lstRow = new List<string>();
                lstRow.Clear();
                foreach (DataColumn col in dt.Columns)
                {
                    if (!row.IsNull(col))
                    {
                        lstRow.Add(row[col].ToString());
                    }
                    else
                    {
                        lstRow.Add(string.Empty);
                    }
                }
                lstTable.Add(string.Join("|", lstRow));
            }
            string datas = string.Join(Environment.NewLine, lstTable);
            return header + Environment.NewLine + datas;
        }

        public static void Log(string content)
        {
            string isWriteLog = Config.GetAppSettingValue("writeLog");
            bool writeLog = false;
            bool.TryParse(isWriteLog, out writeLog);
            if (!writeLog) return;

            string currPath = Environment.CurrentDirectory + "\\Application_" + DateTime.Today.ToString("yyyyMMdd") + ".log";
            File.AppendAllText(currPath, content, Config.Encoding);
        }
        public static string GetArgValue(string cmdHeader, string[] args)
        {
            int x = args.ToList().IndexOf(cmdHeader);
            if (x > -1)
            {
                string v = args[x + 1];
                if (Regex.IsMatch(v, @"^--?[\w\-]+$"))
                {
                    return "";
                }
                else
                {
                    return v;
                }
            }
            else
            {
                return "";
            }
        }
    }
}
