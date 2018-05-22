﻿
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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
        public int[] bitmap;

        // 压缩后的图像数据
        public byte[] compressedData;

        public Screenshot(int width, int height, long captureTime, byte[] compressedData)
        {
            this.width = width;
            this.height = height;
            this.captureTime = captureTime;
            this.compressedData = compressedData;
        }

        public Screenshot(Bitmap img)
        {
            this.captureTime = TimeUtil.Now();
            this.width = img.Width;
            this.height = img.Height;
            this.bitmap = new int[img.Width * img.Height];
            using (MemoryStream stream = new MemoryStream())
            {
                // img.Save(stream, ImageFormat.MemoryBmp);
            }
        }

        // 截屏是否己过期，超过1秒的不需要再发送了
        public bool IsExpired()
        {
            return TimeUtil.Now() - this.captureTime > 1000;
        }
    }
}
