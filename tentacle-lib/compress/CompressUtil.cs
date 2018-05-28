using System;
using System.Collections.Generic;
using System.Text;

namespace cn.org.hentai.tentacle.compress
{
    public class CompressUtil
    {
        /// <summary>
        /// 使用指定的压缩方法进行图像压缩
        /// </summary>
        /// <param name="method">压缩方法，目前只有rle行程编码</param>
        /// <param name="argbArray">待压缩的图像数据，int32的RGB颜色值</param>
        /// <param name="from">图像数据的开始序号</param>
        /// <param name="to">图像数据的结束序号</param>
        /// <returns>压缩后的字节数组</returns>
        public static byte[] process(String method, UInt32[] argbArray, int from, int to)
        {
            return new RLEncoding().compress(argbArray, from, to);
        }
    }
}
