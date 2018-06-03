using cn.org.hentai.tentacle.app.worker;
using cn.org.hentai.tentacle.compress;
using cn.org.hentai.tentacle.hid;
using cn.org.hentai.tentacle.protocol;
using cn.org.hentai.tentacle.system;
using cn.org.hentai.tentacle.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace cn.org.hentai.tentacle.app
{
    public class TentacleApp : Worker
    {
        // 是否处理远程控制状态中
        bool working = false;
        private ByteWriter byteBuffer = null;
        private SocketClient client = null;

        private HeartbeatSender heartbeatSender = null;                 // 心跳发送线程
        private CaptureWorker captureWorker = null;                     // 屏幕截屏线程
        private CompressWorker compressWorker = null;                   // 画面压缩线程

        // 指令包处理器委托声明
        private delegate Packet CommandHandler(Packet packet);
        private CommandHandler[] handlers = new CommandHandler[256];    // 指令包处理器

        public override void before()
        {
            // 加载配置文件
            Configs.init(System.IO.Directory.GetCurrentDirectory() + "\\" + "config.ini");

            // 键盘映射初始化
            KeyMapping.init();

            // RLE压缩处理器初始化
            RLEncoding.init();

            // TODO: 单个可处理包大小不能超过400k
            this.byteBuffer = new ByteWriter(4096 * 100);

            // 包指令处理器
            handlers[Command.HEARTBEAT] = heartbeat;
            handlers[Command.CONTROL_REQUEST] = requestControl;
            handlers[Command.CLOSE_REQUEST] = closeControl;
            handlers[Command.GET_CLIPBOARD] = readClipboard;
            handlers[Command.SET_CLIPBOARD] = writeClipboard;
            handlers[Command.HID_COMMAND] = hidCommandExec;
            handlers[Command.LIST_FILES] = listFiles;
            handlers[Command.DOWNLOAD_FILE] = transferFile;
            handlers[Command.UPLOAD_FILE] = writeFile;
        }

        public override void after()
        {
            if (heartbeatSender != null) heartbeatSender.terminated = true;
            if (captureWorker != null) captureWorker.terminated = true;
            if (compressWorker != null) compressWorker.terminated = true;

            byteBuffer.Close();
        }

        public override void run()
        {
            client = new SocketClient();
            client.connect(Configs.get("server", "localhost"), Configs.getInt("port", 1986), new SocketClient.BufferHandler(bufferHandler));

            (heartbeatSender = new HeartbeatSender(client)).start();

            // TODO: 其它工作线程的启动
            while (true) Thread.Sleep(10000);
        }

        private void bufferHandler(byte[] block)
        {
            // 粘包处理，如果当前缓冲区的数据包数据体大小尚不足一个包，就继续等待
            byteBuffer.Write(block, 0, block.Length);
            int packetLength = ByteUtil.getInt(byteBuffer.buffer, 7, 4) + 11;
            if (byteBuffer.Length < packetLength) return;

            Packet packet = Packet.from(byteBuffer.Cut(packetLength));

            Console.WriteLine("Receive: ");
            ByteUtil.dump(packet.getBytes());

            packet.seek(6);
            int cmd = packet.nextByte() & 0xff;
            int dataLength = packet.nextInt() & 0x7fffffff;

            CommandHandler handler = handlers[cmd];
            if (null == handler) return;
            Packet resp = handler(packet);
            if (resp != null) client.send(resp.getBytes());
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////
        // /////////////////////////////////////////////////////////////////////////////////////////////////
        // 开始远程控制
        public Packet requestControl(Packet packet)
        {
            // TODO: 暂不响应服务器端的控制请求的细节要求，比如压缩方式、带宽、颜色位数等
            int compressMethod = packet.nextByte() & 0xff;
            int bandWidth = packet.nextByte() & 0xff;
            int colorBits = packet.nextByte() & 0xff;
            // TODO: 获取屏幕当前分辨率
            // Dimension screenSize = Toolkit.getDefaultToolkit().getScreenSize();
            short screenWidth = (short)Screen.PrimaryScreen.Bounds.Width;
            short screenHeight = (short)Screen.PrimaryScreen.Bounds.Height;
            Packet resp = Packet.create(Command.CONTROL_RESPONSE, 15)
                    .addByte((byte)0x01)                            // 压缩方式
                    .addByte((byte)0x00)                            // 带宽
                    .addByte((byte)0x03)                            // 颜色位数
                    .addShort((short)screenWidth)                   // 屏幕宽度
                    .addShort((short)screenHeight)                  // 屏幕高度
                    .addLong(DateTime.Now.Ticks / 1000000);         // 当前系统时间戳

            // TODO: 在这里启动工作线程
            (compressWorker = new CompressWorker(this.client)).start();
            (captureWorker = new CaptureWorker(compressWorker)).start();

            return resp;
        }

        // 停止远程控制
        public Packet closeControl(Packet packet)
        {
            if (captureWorker != null) captureWorker.terminated = true;
            if (compressWorker != null) compressWorker.terminated = true;
            // if (heartbeatSender != null) heartbeatSender.terminated = true;
            return Packet.create(Command.CLOSE_RESPONSE, 4).addBytes(Encoding.ASCII.GetBytes("OJBK"));
        }

        // 心跳包响应
        public Packet heartbeat(Packet packet)
        {
            return Packet.create(Command.COMMON_RESPONSE, 4).addBytes(Encoding.ASCII.GetBytes("OJBK"));
        }

        // 获取剪切板内容
        public Packet readClipboard(Packet packet)
        {
            string text = ClipboardAsync.GetText();
            if (text != null && text.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                return Packet.create(Command.GET_CLIPBOARD_RESPONSE, 4 + bytes.Length).addInt(bytes.Length).addBytes(bytes);
            }
            return null;
        }

        // 设置剪切板内容
        public Packet writeClipboard(Packet packet)
        {
            int len = packet.nextInt();
            String text = Encoding.UTF8.GetString(packet.nextBytes(len));
            ClipboardAsync.SetText(text);
            return Packet.create(Command.SET_CLIPBOARD_RESPONSE, 4).addBytes(Encoding.UTF8.GetBytes("OJBK"));
        }

        // 键鼠事件
        public Packet hidCommandExec(Packet packet)
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
                if (eventType == MouseCommand.MOUSE_MOVE) MouseCtrl.mouseMove(x, y);
                else if (eventType == MouseCommand.MOUSE_DOWN) MouseCtrl.mouseDown(x, y, key);
                else if (eventType == MouseCommand.MOUSE_UP) MouseCtrl.mouseUp(x, y, key);
                else if (eventType == MouseCommand.MOUSE_WHEEL) MouseCtrl.mouseScroll(x, y, key);

                Console.WriteLine("MouseEvent: " + eventType);
            }
            else
            {
                hidCommand = new KeyboardCommand(key, eventType, timestamp);
            }
            return null;
        }

        // 列出文件列表
        public Packet listFiles(Packet packet)
        {
            Console.WriteLine("list files.....");
            int len = packet.nextInt();
            String path = Encoding.UTF8.GetString(packet.nextBytes(len));
            cn.org.hentai.tentacle.system.File[] files = null;
            try
            {
                files = FileSystem.list(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
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
                return Packet.create(Command.LIST_FILES_RESPONSE, (int)baos.Length).addBytes(baos.ToArray());
            }
        }

        // 传送文件到服务器端
        public Packet transferFile(Packet packet)
        {
            int pLength = packet.nextInt();
            String path = Encoding.UTF8.GetString(packet.nextBytes(pLength));
            int nLength = packet.nextInt();
            String name = Encoding.UTF8.GetString(packet.nextBytes(nLength));
            // new FileTransferWorker(this, new File(new File(path), name)).start();
            new FileTransferWorker(client, path + name).start();
            return null;
        }

        // 写入文件到本地计算机
        public Packet writeFile(Packet packet)
        {
            return null;
        }
    }
}
