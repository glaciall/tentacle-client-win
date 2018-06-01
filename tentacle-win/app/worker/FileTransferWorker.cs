using cn.org.hentai.tentacle.protocol;
using cn.org.hentai.tentacle.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace cn.org.hentai.tentacle.app.worker
{
    public class FileTransferWorker : Worker
    {
        private String filePath;
        private SocketClient connection;

        public FileTransferWorker(SocketClient conn, String filePath)
        {
            this.connection = conn;
            this.filePath = filePath;
        }

        public override void run()
        {
            int len = -1;
            byte[] block = new byte[40960];
            FileStream fis = null;
            try
            {
                fis = File.OpenRead(filePath);
                while ((len = fis.Read(block, 0, block.Length)) > -1)
                {
                    byte[] data = null;
                    if (len == 40960) data = block;
                    else
                    {
                        data = new byte[len];
                        Array.Copy(block, 0, data, 0, len);
                    }
                    connection.send(Packet.create(Command.DOWNLOAD_FILE_RESPONSE, len + 4).addInt(len).addBytes(data).getBytes());
                }
                connection.send(Packet.create(Command.DOWNLOAD_FILE_RESPONSE, 4).addInt(0).getBytes());
            }
            finally
            {
                try { fis.Close(); } catch (Exception e) { }
            }
        }
    }
}
