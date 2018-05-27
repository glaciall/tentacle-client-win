using cn.org.hentai.tentacle.compress;
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
        TcpClient conn;
        Stream stream;

        private void converse()
        {
            Thread.CurrentThread.Name = "tentacle-server-converse";

            working = false;
            conn = new TcpClient(Configs.get("server", "localhost"), Configs.getInt("port", 1986));
            conn.ReceiveTimeout = 30000;
            conn.SendBufferSize = 30000;
            stream = conn.GetStream();

            lastActiveTime = DateTime.Now.Ticks;
            Console.WriteLine("Connected to server...");

            // TODO 1. 身份验证
            while (true)
            {
                if (DateTime.Now.Ticks - lastActiveTime > 300000000) break;
                // 有无下发下来的数据包
                Packet packet = Packet.read(stream, conn.Available);
                if (packet != null)
                {
                    lastActiveTime = DateTime.Now.Ticks;
                    processCommand(packet);
                }
                
                // TODO: 发送截图

                // 如果闲置超过20秒，则发送一个心跳包
                if (DateTime.Now.Ticks - lastActiveTime > 200000000)
                {
                    Packet p = Packet.create(Command.HEARTBEAT, 5);
                    p.addBytes(Encoding.ASCII.GetBytes("HELLO"));
                    send(p);
                    lastActiveTime = DateTime.Now.Ticks;
                }
                Thread.Sleep(5);
            }
        }

        private void processCommand(Packet packet)
        {
            packet.skip(6);
            int cmd = packet.nextByte();
            int length = packet.nextInt();
            Packet resp = null;

            Console.WriteLine("Recv Cmd: " + cmd);

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
                // (captureWorker = new CaptureWorker()).start();
                // (compressWorker = new CompressWorker()).start();
                // (hidCommandExecutor = new HIDCommandExecutor()).start();
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
                    return;
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
                send(resp);
            }
        }

        public void send(Packet packet)
        {
            stream.Write(packet.getBytes(), 0, packet.getSize());
            stream.Flush();
        }
}
}
