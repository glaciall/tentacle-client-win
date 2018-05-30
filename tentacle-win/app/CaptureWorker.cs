using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cn.org.hentai.tentacle.app
{
    public class CaptureWorker : Worker
    {
        public CaptureWorker()
        {
            // ...
        }

        public override void run()
        {
            // 截一张屏
            // 放到队列里去
            // 要不要通知其它工作线程
        }

        public override int loops()
        {
            return -1;
        }

        public override int loopInterval()
        {
            return 50;
        }
    }
}
