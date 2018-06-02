using cn.org.hentai.tentacle.graphic;
using cn.org.hentai.tentacle.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using cn.org.hentai.tentacle.compress;
using cn.org.hentai.tentacle.protocol;
using System.Threading;

namespace cn.org.hentai.tentacle.app
{
    public class CompressWorker : Worker
    {
        private ConcurrentQueue<Screenshot> screenshots;
        private SocketClient client;
        private Screenshot lastScreen = null;
        private int sequence = 0;

        public CompressWorker(SocketClient socketClient)
        {
            this.client = socketClient;
            this.screenshots = new ConcurrentQueue<Screenshot>();
        }

        public void addScreenshot(Screenshot screenshot)
        {
            this.screenshots.Enqueue(screenshot);
        }

        public override void run()
        {
            Screenshot screenshot = null;
            if (this.screenshots.TryDequeue(out screenshot) == false)
            {
                return;
            }
            if (screenshot == null)
            {
                return;
            }

            // 分辨率是否发生了变化？
            if (lastScreen != null && (lastScreen.width != screenshot.width || lastScreen.height != screenshot.height)) lastScreen = null;

            // 1. 求差
            UInt32[] bitmap = new UInt32[screenshot.width * screenshot.height];
            int changedColors = 0, start = -1, end = bitmap.Length;
            if (lastScreen != null)
            {
                for (int i = 0; i < bitmap.Length; i++)
                {
                    if (lastScreen.bitmap[i] == screenshot.bitmap[i])
                    {
                        bitmap[i] = 0;
                    }
                    else
                    {
                        if (start == -1) start = i;
                        else end = i;
                        changedColors += 1;
                        lastScreen.bitmap[i] = bitmap[i] = screenshot.bitmap[i];
                    }
                }
            }
            else bitmap = screenshot.bitmap;

            if (lastScreen != null && changedColors == 0)
            {
                return;
            }
            // Console.WriteLine("Changed colors: " + changedColors);

            // 2. 压缩
            start = 0;
            end = screenshot.width * screenshot.height;
            byte[] compressedData = CompressUtil.process("rle", bitmap, start, end);

            // 3. 发送到服务器端
            Packet packet = Packet.create(Command.SCREENSHOT, compressedData.Length + 16);
            packet.addShort((short)screenshot.width)
                    .addShort((short)screenshot.height)
                    .addLong(screenshot.captureTime)
                    .addInt(sequence++);
            packet.addBytes(compressedData);

            client.send(packet.getBytes());

            if (null == lastScreen)
            {
                UInt32[] bmp = new UInt32[screenshot.width * screenshot.height];
                Array.Copy(screenshot.bitmap, bmp, bmp.Length);
                lastScreen = new Screenshot(bmp, screenshot.width, screenshot.height);
            }
        }

        public override int loops()
        {
            return -1;
        }

        public override int loopInterval()
        {
            return 20;
        }
    }
}
