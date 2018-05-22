using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.hid
{
    public class KeyMapping
    {
        private static byte[] mappings = new byte[256];

        public static void init()
        {
            // TODO: 按钮映射
        }

        public static int convert(int code)
        {
            int key = code & 0xff;
            if (mappings[code] > 0) key = mappings[code];
            return key;
        }
    }
}
