
using System;
using System.IO;

using cn.org.hentai.tentacle.util;

namespace cn.org.hentai.tentacle.graphic
{
    public class Screenshot
    {
        // 截屏时间
        public long captureTime;

        // 宽度
        public int width;

        // 高度
        public int height;

        // RGB
        public UInt32[] bitmap;

        // 压缩后的图像数据
        public byte[] compressedData;

        public Screenshot(int width, int height, long captureTime, byte[] compressedData)
        {
            this.width = width;
            this.height = height;
            this.captureTime = captureTime;
            this.compressedData = compressedData;
        }

        public unsafe Screenshot(UInt32[] bitmap, int width, int height)
        {
            this.captureTime = TimeUtil.Now();
            this.width = width;
            this.height = height;
            this.bitmap = bitmap;
        }

        // 截屏是否己过期，超过1秒的不需要再发送了
        public bool isExpired()
        {
            return TimeUtil.Now() - this.captureTime > 1000;
        }
    }
}
