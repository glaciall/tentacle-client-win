using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.util
{
    public class TimeUtil
    {
        public static long Now()
        {
            return DateTime.Now.Ticks / 10000;
        }
    }
}
