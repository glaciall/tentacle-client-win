using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.hid
{
    public class KeyboardCommand : HIDCommand
    {
        public const int KEY_PRESS = 0x01;
        public const int KEY_RELEASE = 0x02;

        public int keycode;
        public int eventType;
        public KeyboardCommand(int keycode, int eventType, int timestamp) : base(TYPE_KEYBOARD, timestamp)
        {
            this.keycode = keycode;
            this.eventType = eventType;
        }
    }
}
