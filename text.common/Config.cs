using System;
using System.Text;
using System.Configuration;

namespace Text.Common
{
    public class Config
    {
        public static string GetAppSettingValue(string key)
        {
            return GetAppSettingValue2(key, string.Empty);
        }
        public static string GetAppSettingValue2(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings.Get(key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            else
            {
                return value;
            }
        }

        public static Encoding Encoding
        {
            get
            {
                string encoding = GetAppSettingValue("encoding");
                //Encoding = New System.Text.UTF8Encoding(False)
                if (string.IsNullOrEmpty(encoding))
                {
                    return new UTF8Encoding(false);
                }
                
                if (encoding.Equals("utf-8",StringComparison.OrdinalIgnoreCase))
                {
                    return new UTF8Encoding(false);
                }
                else
                {
                    return Encoding.GetEncoding(encoding); ;
                }
            }
        }
    }
}
