using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace cn.org.hentai.tentacle.system
{
    /// <summary>
    /// 负责截屏、鼠标控制、按键模拟
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="key">键码</param>
        /// <returns>是否发送成功</returns>
        public bool mousePress(int key)
        {
            return false;
        }

        /// <summary>
        /// 鼠标按键松开
        /// </summary>
        /// <param name="key">键码</param>
        /// <returns>是否发送成功</returns>
        public bool mouseRelease(int key)
        {
            return false;
        }

        /// <summary>
        /// 鼠标滚轮滚动
        /// </summary>
        /// <param name="direction">方向，待定</param>
        /// <returns>是否发送成功</returns>
        public bool mouseWheelScroll(int direction)
        {
            return false;
        }

        /// <summary>
        /// 鼠标定位至指定位置，无中间过程
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>是否发送成功</returns>
        public bool mouseMoveTo(int x, int y)
        {
            return false;
        }

        /// <summary>
        /// 按键松开
        /// </summary>
        /// <param name="keycode">键码</param>
        /// <returns>是否发送成功</returns>
        public bool keyRelease(int keycode)
        {
            return false;
        }

        /// <summary>
        /// 按键按下
        /// </summary>
        /// <param name="keycode">键码</param>
        /// <returns>是否发送成功</returns>
        public bool keyPress(int keycode)
        {
            return false;
        }

        /// <summary>
        /// 截屏
        /// </summary>
        /// <returns></returns>
        public static Int32[] captureScreen()
        {
            return null;
        }

        /// <summary>
        /// 获取屏幕分辨率
        /// </summary>
        /// <returns></returns>
        public static Rectangle getScreenSize()
        {
            return new Rectangle(0, 0, 0, 0);
        }

        /// <summary>
        /// TODO: 初始化
        /// </summary>
        public static void init()
        {
            // ..
        }
    }
}
