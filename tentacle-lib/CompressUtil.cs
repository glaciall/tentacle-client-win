using System;
using System.Collections.Generic;
using System.Text;

namespace cn.org.hentai.tentacle.compress
{
    public class CompressUtil
    {
        public static byte[] process(String method, int[] argbArray, int from, int to)
        {
            return new RLEncoding().compress(argbArray, from, to);
        }
    }
}
