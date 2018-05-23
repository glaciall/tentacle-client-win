using System;
using System.Collections.Generic;
using System.Text;

namespace cn.org.hentai.tentacle.compress
{
    public abstract class BaseCompressProcessor
    {
        public abstract byte[] compress(int[] bitmap, int from, int to);
    }
}
