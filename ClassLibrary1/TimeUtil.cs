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
            DateTime nTime = new DateTime().AddHours(-8);
            long ctime = nTime.Ticks;
            long c1970 = new DateTime(1970, 1, 1).Ticks;
            return (ctime - c1970) / 10000;
        }
    }
}
