using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace cn.org.hentai.tentacle.util
{
    /// <summary>
    /// 用于实现与java的ByteArrayOutputStream一样的功能
    /// </summary>
    public class ByteWriter : Stream
    {
        private byte[] buffer;                      // 数据存储区
        private long offset = 0;                    // 数据写入的当前位置指针
        private long size = 0;                      // 实际写入大小

        public ByteWriter(int capacity)
        {
            this.buffer = new byte[capacity];
        }

        public override bool CanRead { get { return false; } }
        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override long Length { get { return size; } }

        public override long Position { get { return offset; } set {  } }

        public override void Flush()
        {
            // do nothing...
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // if (origin != SeekOrigin.Begin) throw new Exception("only absolute position seek supported");
            // return offset;
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++) this.buffer[offset++] = buffer[i + offset];
            this.size += count;
        }

        public override void WriteByte(byte value)
        {
            this.buffer[this.offset++] = value;
            this.size += 1;
        }

        public void Reset()
        {
            this.offset = 0;
            this.size = 0;
        }

        public byte[] ToArray()
        {
            byte[] data = new byte[this.size];
            Array.Copy(this.buffer, data, size);
            return data;
        }
    }
}
