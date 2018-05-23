using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.hid
{
    public class HIDCommand
    {
        public const int TYPE_MOUSE = 1;
        public const int TYPE_KEYBOARD = 2;

        public int type;
        public int timestamp;

        public HIDCommand(int type, int timestamp)
        {
            this.type = type;
            this.timestamp = timestamp;
        }
    }
}
