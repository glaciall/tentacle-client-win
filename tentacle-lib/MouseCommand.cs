using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.hid
{
    public class MouseCommand : HIDCommand
    {
        public const int MOUSE_DOWN = 0x01;
        public const int MOUSE_UP = 0x02;
        public const int MOUSE_MOVE = 0x03;
        public const int MOUSE_WHEEL = 0x04;

        public int eventType;           // 事件类型，按下，放开，移动
        public int key;                 // 1左键，2中键，3右键，或1 向上，2向下
        public int x;
        public int y;

        public MouseCommand(int eventType, int key, int x, int y, int timestamp) : base(TYPE_MOUSE, timestamp)
        {
            this.eventType = eventType & 0xff;
            this.key = key & 0xff;
            this.x = x;
            this.y = y;
        }
    }
}
