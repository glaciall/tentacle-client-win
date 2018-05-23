using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.util
{
    public class Configs
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private const string section = "tentacle";
        private static string configFilePath;

        public static void init(String filePath)
        {
            configFilePath = filePath;
        }

        public static String get(String key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", temp, 255, configFilePath);
            return temp.ToString();
        }

        public static int getInt(String key, int defaultVal)
        {
            String val = get(key);
            if (null == val) return defaultVal;
            else return Convert.ToInt32(val);
        }
    }
}
