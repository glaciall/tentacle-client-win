using cn.org.hentai.tentacle.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.protocol
{
    public class Packet
    {
        // 头部往后的数据体长度
        int size = 0;
        // 当前读写指针索引位置
        int offset = 0;
        // 包数据体最大大小
        int maxSize = 0;
        // 头部+数据体
        public byte[] data;

        private Packet()
        {
            // do nothing here..
        }

        public static Packet from(byte[] data)
        {
            Packet packet = new Packet();
            packet.data = data;
            packet.size = data.Length - 11;
            return packet;
        }

        public static Packet read(Stream stream, int bytesInBuffer)
        {
            if (bytesInBuffer < 11) return null;
            byte[] head = new byte[11];
            int len = stream.Read(head, 0, 11);
            int dataLength = ByteUtil.getInt(head, 7, 4) & 0x7fffff;
            using (MemoryStream ms = new MemoryStream(dataLength + 10))
            {
                byte[] buff = new byte[512];
                for (int i = 0; i < dataLength; i += len)
                {
                    len = stream.Read(buff, 0, Math.Min(512, dataLength - i));
                    if (len == -1) break;
                    ms.Write(buff, 0, len);
                }
                Packet p = new Packet();
                p.data = new byte[dataLength + 6 + 1 + 4];
                p.size = 0;
                p.maxSize = p.size;
                p.addBytes(head);
                p.addBytes(ms.ToArray());
                buff = null;
                head = null;
                return p;
            }
        }

        /**
         * 创建协议数据包
         * @param command 指令，参见cn.org.hentai.tentacle.protocol.Command类
         * @param length 数据包的长度
         * @return
         */
        public static Packet create(byte command, int length)
        {
            Packet p = new Packet();
            p.data = new byte[length + 6 + 1 + 4];
            p.data[0] = (byte)'H';
            p.data[1] = (byte)'E';
            p.data[2] = (byte)'N';
            p.data[3] = (byte)'T';
            p.data[4] = (byte)'A';
            p.data[5] = (byte)'I';
            p.data[6] = command;
            Array.Copy(ByteUtil.toBytes(length), 0, p.data, 7, 4);
            p.size = 11;
            p.maxSize = length;
            return p;
        }

        public int getSize()
        {
            return this.size;
        }

        public Packet addByte(byte b)
        {
            this.data[size++] = b;
            return this;
        }

        public Packet addShort(short s)
        {
            this.data[size++] = (byte)((s >> 8) & 0xff);
            this.data[size++] = (byte)(s & 0xff);
            return this;
        }

        public Packet addInt(int i)
        {
            this.data[size++] = (byte)((i >> 24) & 0xff);
            this.data[size++] = (byte)((i >> 16) & 0xff);
            this.data[size++] = (byte)((i >> 8) & 0xff);
            this.data[size++] = (byte)(i & 0xff);
            return this;
        }

        public Packet addLong(long l)
        {
            this.data[size++] = (byte)((l >> 56) & 0xff);
            this.data[size++] = (byte)((l >> 48) & 0xff);
            this.data[size++] = (byte)((l >> 40) & 0xff);
            this.data[size++] = (byte)((l >> 32) & 0xff);
            this.data[size++] = (byte)((l >> 24) & 0xff);
            this.data[size++] = (byte)((l >> 16) & 0xff);
            this.data[size++] = (byte)((l >> 8) & 0xff);
            this.data[size++] = (byte)(l & 0xff);
            return this;
        }

        public Packet addBytes(byte[] b)
        {
            Array.Copy(b, 0, this.data, size, b.Length);
            size += b.Length;
            return this;
        }

        public Packet rewind()
        {
            this.offset = 0;
            return this;
        }

        public byte nextByte()
        {
            return this.data[offset++];
        }

        public short nextShort()
        {
            return (short)(((this.data[offset++] & 0xff) << 8) | (this.data[offset++] & 0xff));
        }

        public int nextInt()
        {
            return (this.data[offset++] & 0xff) << 24 | (this.data[offset++] & 0xff) << 16 | (this.data[offset++] & 0xff) << 8 | (this.data[offset++] & 0xff);
        }

        public long nextLong()
        {
            return ((long)this.data[offset++] & 0xff) << 56
                    | ((long)this.data[offset++] & 0xff) << 48
                    | ((long)this.data[offset++] & 0xff) << 40
                    | ((long)this.data[offset++] & 0xff) << 32
                    | ((long)this.data[offset++] & 0xff) << 24
                    | ((long)this.data[offset++] & 0xff) << 16
                    | ((long)this.data[offset++] & 0xff) << 8
                    | ((long)this.data[offset++] & 0xff);
        }

        public byte[] nextBytes(int length)
        {
            byte[] buf = new byte[length];
            Array.Copy(this.data, offset, buf, 0, length);
            offset += length;
            return buf;
        }

        public Packet skip(int offset)
        {
            this.offset += offset;
            return this;
        }

        public Packet seek(int index)
        {
            this.offset = index;
            return this;
        }

        public byte[] getBytes()
        {
            if (size == maxSize) return this.data;
            else
            {
                byte[] buff = new byte[size];
                Array.Copy(this.data, 0, buff, 0, size);
                return buff;
            }
        }
    }
}
