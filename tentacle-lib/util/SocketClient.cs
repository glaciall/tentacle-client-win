using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace cn.org.hentai.tentacle.util
{
    public class SocketClient
    {
        private byte[] buff;
        private int receiveBufferSize = 512;
        private int sendBufferSize = 512;

        private int connectTimeout = 10000;

        private TcpClient client = null;
        private Socket socket = null;

        public delegate void BufferHandler(byte[] data);

        private BufferHandler bufferHandler;

        public void setConnectTimeout(int milliseconds)
        {
            this.connectTimeout = milliseconds;
        }

        public void setReceiveBufferSize(int size)
        {
            this.receiveBufferSize = size;
            if (socket != null) socket.ReceiveBufferSize = size;
        }

        public void setSendBufferSize(int size)
        {
            this.sendBufferSize = size;
            if (socket != null) socket.SendBufferSize = size;
        }

        public void connect(String host, int port, BufferHandler bufferHandler)
        {
            if (this.client != null) throw new Exception("连接己打开");
            this.client = new TcpClient(host, port);
            /*
            IAsyncResult ar = this.client.BeginConnect(host, port, new AsyncCallback(connectCallback), this.client);
            if (ar.AsyncWaitHandle.WaitOne() == false)
            {
                this.close();
                throw new Exception("连接超时");
            }
            */
            this.socket = this.client.Client;
            this.socket.Blocking = false;
            this.bufferHandler = bufferHandler;

            this.buff = new byte[receiveBufferSize];
            tryReceive();
        }

        private void connectCallback(IAsyncResult ar)
        {
            TcpClient conn = (TcpClient)ar.AsyncState;
            Console.WriteLine("xx: " + conn.Connected);
            conn.EndConnect(ar);
        }

        private void onReceive(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;
            int len = socket.EndReceive(ar);
            byte[] data = new byte[len];
            Array.Copy(buff, data, len);
            this.bufferHandler(data);
            if (socket != null && socket.Connected)
            {
                tryReceive();
            }
        }

        private void tryReceive()
        {
            try
            {
                socket.BeginReceive(buff, 0, buff.Length, 0, new AsyncCallback(onReceive), socket);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Socket: " + e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        public void send(byte[] data)
        {
            socket.BeginSend(data, 0, data.Length, 0, null, socket);
        }

        public void close()
        {
            try
            {
                client.Close();
            }
            catch { }
            client = null;
            Console.WriteLine("己经关闭了？");
        }
    }
}
