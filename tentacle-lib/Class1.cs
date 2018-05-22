using cn.org.hentai.tentacle.util;
using System;
using System.Drawing;

namespace ClassLibrary1
{
    public class Class1
    {
        // 截屏时间
        public long captureTime;

        // 宽度
        public int width;

        // 高度
        public int height;

        // RGB
        public int[] bitmap;

        // 压缩后的图像数据
        public byte[] compressedData;

        public Class1(int width, int height, long captureTime, byte[] compressedData)
        {
            this.width = width;
            this.height = height;
            this.captureTime = captureTime;
            this.compressedData = compressedData;
        }

        public Class1()
        {
            
        }

        // 截屏是否己过期，超过1秒的不需要再发送了
        public bool IsExpired()
        {
            return TimeUtil.Now() - this.captureTime > 1000;
        }
    }
}
