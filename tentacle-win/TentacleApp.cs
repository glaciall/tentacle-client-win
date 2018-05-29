using cn.org.hentai.tentacle.compress;
using cn.org.hentai.tentacle.display;
using cn.org.hentai.tentacle.graphic;
using cn.org.hentai.tentacle.hid;
using cn.org.hentai.tentacle.protocol;
using cn.org.hentai.tentacle.system;
using cn.org.hentai.tentacle.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using tentacle_lib.util;

namespace cn.org.hentai.tentacle.app
{
    public class TentacleApp
    {
        private long lastActiveTime = 0L;
        private static Thread converseThread;

        public TentacleApp()
        {
            // ..
        }

        public void start()
        {
            // 加载配置文件
            Configs.init(System.IO.Directory.GetCurrentDirectory() + "\\" + "config.ini");

            // 键盘映射初始化
            KeyMapping.init();

            // RLE压缩处理器初始化
            RLEncoding.init();

            // 通信守护线程
            new Thread(connectionDaemon).Start();
        }

        private void connectionDaemon()
        {
            Thread.CurrentThread.Name = "tentacle-session-daemon";
            while (true)
            {
                if (DateTime.Now.Ticks - lastActiveTime > 1000 * 1000 * 60 * 5)
                {
                    if (converseThread != null)
                    {
                        try
                        {
                            converseThread.Interrupt();
                        }
                        catch (Exception e) { }
                    }
                    converseThread = new Thread(converse);
                    converseThread.Start();
                }
                Thread.Sleep(5000);
            }
        }

        bool working = false;
        Object messenger = new Object();
        TcpClient conn;
        Stream stream;
        LinkedList<Screenshot> screenshots = new LinkedList<Screenshot>();
        LinkedList<Packet> compressedScreenshots = new LinkedList<Packet>();

        private void converse()
        {
            Thread.CurrentThread.Name = "tentacle-server-converse";

            working = false;
            try
            {
                conn = new TcpClient(Configs.get("server", "localhost"), Configs.getInt("port", 1986));
            }
            catch
            {
                Console.WriteLine("cannot connect to server...");
                return;
            }
            conn.ReceiveTimeout = 30000;
            conn.SendBufferSize = 30000;
            stream = conn.GetStream();

            lastActiveTime = DateTime.Now.Ticks;
            Console.WriteLine("Connected to server...");

            // TODO 1. 身份验证
            while (conn.Connected)
            {
                if (DateTime.Now.Ticks - lastActiveTime > 300000000) break;
                // 有无下发下来的数据包
                Packet packet = Packet.read(stream, conn.Available);
                if (packet != null)
                {
                    lastActiveTime = DateTime.Now.Ticks;
                    if (processCommand(packet) == false) break;
                }
                
                // TODO: 发送截图

                // 如果闲置超过20秒，则发送一个心跳包
                if (DateTime.Now.Ticks - lastActiveTime > 200000000)
                {
                    Packet p = Packet.create(Command.HEARTBEAT, 5);
                    p.addBytes(Encoding.ASCII.GetBytes("HELLO"));
                    if (send(p) == false) break;
                    lastActiveTime = DateTime.Now.Ticks;
                }
                Thread.Sleep(5);
            }

            working = false;
        }

        private bool processCommand(Packet packet)
        {
            packet.skip(6);
            int cmd = packet.nextByte();
            int length = packet.nextInt();
            Packet resp = null;

            // 心跳
            if (cmd == Command.HEARTBEAT)
            {
                resp = Packet.create(Command.COMMON_RESPONSE, 4).addByte((byte)'O').addByte((byte)'J').addByte((byte)'B').addByte((byte)'K');
            }
            // 开始远程控制
            else if (cmd == Command.CONTROL_REQUEST)
            {
                if (working) throw new Exception("Already working on capture screenshots...");
                working = true;

                // TODO: 暂不响应服务器端的控制请求的细节要求，比如压缩方式、带宽、颜色位数等
                int compressMethod = packet.nextByte() & 0xff;
                int bandWidth = packet.nextByte() & 0xff;
                int colorBits = packet.nextByte() & 0xff;
                // TODO: 获取屏幕当前分辨率
                // Dimension screenSize = Toolkit.getDefaultToolkit().getScreenSize();
                short screenWidth = (short)Screen.PrimaryScreen.Bounds.Width;
                short screenHeight = (short)Screen.PrimaryScreen.Bounds.Height;
                resp = Packet.create(Command.CONTROL_RESPONSE, 15)
                        .addByte((byte)0x01)                            // 压缩方式
                        .addByte((byte)0x00)                            // 带宽
                        .addByte((byte)0x03)                            // 颜色位数
                        .addShort((short)screenWidth)                   // 屏幕宽度
                        .addShort((short)screenHeight)                  // 屏幕高度
                        .addLong(DateTime.Now.Ticks / 1000000);         // 当前系统时间戳

                new Thread(captureScreen).Start();
                new Thread(compress).Start();
            }
            // 获取剪切板内容
            else if (cmd == Command.GET_CLIPBOARD)
            {
                string text = ClipboardAsync.GetText();
                if (text != null && text.Length > 0)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(text);
                    resp = Packet.create(Command.GET_CLIPBOARD_RESPONSE, 4 + bytes.Length).addInt(bytes.Length).addBytes(bytes);
                }
            }
            // 设置剪切板内容
            else if (cmd == Command.SET_CLIPBOARD)
            {
                int len = packet.nextInt();
                String text = Encoding.UTF8.GetString(packet.nextBytes(len));
                ClipboardAsync.SetText(text);
                resp = Packet.create(Command.SET_CLIPBOARD_RESPONSE, 4).addBytes(Encoding.UTF8.GetBytes("OJBK"));
            }
            // 键鼠事件处理
            else if (cmd == Command.HID_COMMAND)
            {
                int hidType = packet.nextByte() & 0xff;
                int eventType = packet.nextByte() & 0xff;
                int key = packet.nextByte() & 0xff;
                short x = packet.nextShort();
                short y = packet.nextShort();
                int timestamp = packet.nextInt() & 0x7fffffff;
                HIDCommand hidCommand = null;
                if (hidType == HIDCommand.TYPE_MOUSE)
                {
                    hidCommand = new MouseCommand(eventType, key, x, y, timestamp);
                }
                else
                {
                    hidCommand = new KeyboardCommand(key, eventType, timestamp);
                }
                // TODO: HID指令入队列
                // hidCommandExecutor.add(hidCommand);
            }
            // 停止远程控制
            else if (cmd == Command.CLOSE_REQUEST)
            {
                resp = Packet.create(Command.CLOSE_RESPONSE, 4).addBytes(Encoding.ASCII.GetBytes("OJBK"));
                working = false;
                // captureWorker.terminate();
                // compressWorker.terminate();
                // hidCommandExecutor.terminate();
                // ScreenImages.clear();
            }
            // 列出文件列表
            else if (cmd == Command.LIST_FILES)
            {
                int len = packet.nextInt();
                String path = Encoding.UTF8.GetString(packet.nextBytes(len));
                cn.org.hentai.tentacle.system.File[] files = null;
                try
                {
                    files = FileSystem.list(path);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    return true;
                }
                using (MemoryStream baos = new MemoryStream(40960))
                {
                    for (int i = 0; files != null && i < files.Length; i++)
                    {
                        cn.org.hentai.tentacle.system.File file = files[i];
                        // 是否目录，长度，权限
                        String name = file.name;
                        // if ("".Equals(name)) name = file.getAbsolutePath();
                        byte[] fbytes = Encoding.UTF8.GetBytes(name);
                        baos.WriteByte((byte)(file.isDirectory ? 1 : 0));
                        baos.Write(ByteUtil.toBytes(file.length), 0, 8);
                        baos.Write(ByteUtil.toBytes(file.lastModifiedTime), 0, 8);
                        baos.Write(ByteUtil.toBytes(fbytes.Length), 0, 4);
                        baos.Write(fbytes, 0, fbytes.Length);
                    }
                    resp = Packet.create(Command.LIST_FILES_RESPONSE, (int)baos.Length).addBytes(baos.ToArray());
                }
            }
            // 传送文件到服务器端
            else if (cmd == Command.DOWNLOAD_FILE)
            {
                /*
                int pLength = packet.nextInt();
                String path = new String(packet.nextBytes(pLength), "UTF-8");
                int nLength = packet.nextInt();
                String name = new String(packet.nextBytes(nLength), "UTF-8");
                new FileTransferWorker(this, new File(new File(path), name)).start();
                */
            }

            // 发送响应至服务器端
            if (resp != null)
            {
                return send(resp);
            }
            return true;
        }

        // 截屏工作线程
        private void captureScreen()
        {
            lock(screenshots)
            {
                screenshots.Clear();
            }
            while (working)
            {
                Screenshot screenshot = DisplayContext.CaptureScreen();
                lock(screenshots)
                {
                    screenshots.AddLast(screenshot);
                }
                Thread.Sleep(50);
            }
        }

        // 压缩处理工作线程
        private void compress()
        {
            int sequence = 0;
            Screenshot lastScreen = null;

            lock(screenshots)
            {
                screenshots.Clear();
            }

            while (working)
            {
                Screenshot screenshot = null;
                while (true)
                {
                    if (screenshots.Count == 0) break;
                    lock(screenshots)
                    {
                        screenshot = screenshots.First();
                        screenshots.RemoveFirst();
                    }
                }
                if (screenshot == null || screenshot.isExpired())
                {
                    Thread.Sleep(20);
                    continue;
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
                    // Thread.Sleep(20);
                    // Console.WriteLine("no changed colors");
                    // continue;
                }
                Console.WriteLine("Changed colors: " + changedColors);

                // 2. 压缩
                // start = Math.Max(start, 0);
                // start = 0;
                start = 0;
                end = screenshot.width * screenshot.height;
                // end = bitmap.Length;
                byte[] compressedData = CompressUtil.process("rle", bitmap, start, end);

                // Log.debug("Compress Ratio: " + (screenshot.bitmap.length * 4.0f / compressedData.length));
                // Log.debug("After: " + (compressedData.length / 1024));

                // 3. 入队列
                Packet packet = Packet.create(Command.SCREENSHOT, compressedData.Length + 16);
                packet.addShort((short)screenshot.width)
                        .addShort((short)screenshot.height)
                        .addLong(screenshot.captureTime)
                        .addInt(sequence++);
                packet.addBytes(compressedData);

                /*
                lock(compressedScreenshots)
                {
                    compressedScreenshots.AddLast(packet);
                }
                */
                send(packet);

                if (null == lastScreen)
                {
                    UInt32[] bmp = new UInt32[screenshot.width * screenshot.height];
                    Array.Copy(screenshot.bitmap, bmp, bmp.Length);
                    lastScreen = new Screenshot(bmp, screenshot.width, screenshot.height);
                }
                // else lastScreen.bitmap = bitmap;
                Thread.Sleep(20);
            }
        }

        // 文件传输工作线程


        // HID指令执行线程


        public bool send(Packet packet)
        {
            try
            {
                lock (messenger)
                {
                    stream.Write(packet.getBytes(), 0, packet.getSize());
                    stream.Flush();
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Send: " + ex.Message);
                return false;
            }
        }
    }
}
