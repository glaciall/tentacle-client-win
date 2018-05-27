using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        private static Hashtable hashTable;

        public static void init(string filePath)
        {
            StringReader reader = null;
            FileStream fileReader = null;
            MemoryStream ms = null;
            try
            {
                hashTable = new Hashtable();
                fileReader = File.OpenRead(filePath);
                ms = new MemoryStream(1024);
                int len = -1;
                byte[] buff = new byte[512];
                while (true)
                {
                    len = fileReader.Read(buff, 0, 512);
                    if (len <= 0) break;
                    ms.Write(buff, 0, len);
                }
                reader = new StringReader(Encoding.UTF8.GetString(ms.ToArray()));
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    int idx = line.IndexOf('=');
                    if (idx == -1) continue;
                    string key = line.Substring(0, idx).Trim();
                    string value = line.Substring(idx + 1).Trim();
                    hashTable.Add(key, value);
                }
            }
            finally
            {
                try { reader.Close(); } catch { };
                try { ms.Close(); } catch { };
                try { fileReader.Close(); } catch { };
            }
        }

        public static string get(string key, string defaultVal)
        {
            if (!hashTable.Contains(key)) return defaultVal;
            return (string)hashTable[key];
        }

        public static int getInt(string key, int defaultVal)
        {
            string val = get(key, null);
            if (null == val) return defaultVal;
            return Convert.ToInt32(val);
        }
    }
}
