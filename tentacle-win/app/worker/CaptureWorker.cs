using cn.org.hentai.tentacle.display;
using cn.org.hentai.tentacle.graphic;
using cn.org.hentai.tentacle.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace cn.org.hentai.tentacle.app
{
    public class CaptureWorker : Worker
    {
        private CompressWorker compressWorker = null;
        public CaptureWorker(CompressWorker compressWorker)
        {
            this.compressWorker = compressWorker;
        }

        public override void run()
        {
            Screenshot screenshot = DisplayContext.CaptureScreen();
            compressWorker.addScreenshot(screenshot);
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
