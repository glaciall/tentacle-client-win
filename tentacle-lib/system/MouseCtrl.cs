using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace cn.org.hentai.tentacle.system
{
    public class MouseCtrl
    {
        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        const int MOUSEEVENTF_MOVE = 0x1;                   // 鼠标称动
        const int MOUSEEVENTF_LEFTDOWN = 0x2;               // 鼠标左键按下
        const int MOUSEEVENTF_LEFTUP = 0x4;                 // 鼠标左键松开
        const int MOUSEEVENTF_RIGHTDOWN = 0x8;              // 鼠标右键按下
        const int MOUSEEVENTF_RIGHTUP = 0x10;               // 鼠标右键按下
        const int MOUSEEVENTF_MIDDLEDOWN = 0x20;            // 鼠标中键按下
        const int MOUSEEVENTF_MIDDLEUP = 0x40;              // 鼠标中键松开
        const int MOUSEEVENTF_WHEEL = 0x800;                // 鼠标滚轮滚动
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;            // 是否以屏幕的绝对位置指示的位置

        // 自定义的鼠标事件常量
        public const int MOUSE_DOWN = 0x01;
        public const int MOUSE_UP = 0x02;
        public const int MOUSE_MOVE = 0x03;
        public const int MOUSE_SCROLL = 0x04;

        // 自定义的鼠标按钮常量
        public const int BUTTON_LEFT = 0x01;
        public const int BUTTON_RIGHT = 0x02;
        public const int MOUSE_WHEEL = 0x03;

        struct Point
        {
            public int x;
            public int y;
        }

        private static Point convert(int x, int y)
        {
            var p = new Point();
            p.x = x * 65536 / Screen.PrimaryScreen.Bounds.Width;
            p.y = y * 65536 / Screen.PrimaryScreen.Bounds.Height;
            return p;
        }

        public static void mouseMove(int x, int y)
        {
            var p = convert(x, y);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, p.x, p.y, 0, 0);
        }

        public static void mouseUp(int x, int y, int key)
        {
            var p = convert(x, y);
            int btn = MOUSEEVENTF_LEFTUP;
            if (key == BUTTON_RIGHT) btn = MOUSEEVENTF_RIGHTUP;
            mouse_event(btn | MOUSEEVENTF_ABSOLUTE, p.x, p.y, 0, 0);
        }

        public static void mouseDown(int x, int y, int key)
        {
            var p = convert(x, y);
            int btn = MOUSEEVENTF_LEFTDOWN;
            if (key == BUTTON_RIGHT) btn = MOUSEEVENTF_RIGHTDOWN;
            mouse_event(btn | MOUSEEVENTF_ABSOLUTE, p.x, p.y, 0, 0);
        }

        public static void mouseScroll(int x, int y, int direction)
        {
            var p = convert(x, y);
            mouse_event(MOUSEEVENTF_WHEEL | MOUSEEVENTF_ABSOLUTE, p.x, p.y, direction == 1 ? -100 : 100, 0);
        }
    }
}
