using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.util
{
    public class ByteUtil
    {
        public static byte[] parse(String hexString)
        {
            String[] hexes = hexString.Split(' ');
            byte[] data = new byte[hexes.Length];
            for (int i = 0; i < hexes.Length; i++) data[i] = (byte)(Convert.ToByte(hexes[i], 16) & 0xff);
            return data;
        }

        public static String toString(byte[] data)
        {
            return toString(data, data.Length);
        }

        public static String toString(byte[] buff, int length)
        {
            StringBuilder sb = new StringBuilder(length * 2);
            for (int i = 0; i < length; i++)
            {
                if ((buff[i] & 0xff) < 0x10) sb.Append('0');
                sb.Append(Convert.ToString(buff[i] & 0xff, 16).ToUpper());
                sb.Append(' ');
            }
            return sb.ToString();
        }

        public static bool getBit(int val, int pos)
        {
            return getBit(new byte[] {
                (byte)((val >> 0) & 0xff),
                (byte)((val >> 8) & 0xff),
                (byte)((val >> 16) & 0xff),
                (byte)((val >> 24) & 0xff)
            }, pos);
        }

        public static int reverse(int val)
        {
            byte[] bytes = toBytes(val);
            byte[] ret = new byte[4];
            for (int i = 0; i < 4; i++) ret[i] = bytes[3 - i];
            return toInt(ret);
        }

        public static int toInt(byte[] bytes)
        {
            int val = 0;
            for (int i = 0; i < 4; i++) val |= (bytes[i] & 0xff) << ((3 - i) * 8);
            return val;
        }

        public static byte[] toBytes(int val)
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = (byte)(val >> ((3 - i) * 8) & 0xff);
            }
            return bytes;
        }

        public static byte[] toBytes(long val)
        {
            byte[] bytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                bytes[i] = (byte)(val >> ((7 - i) * 8) & 0xff);
            }
            return bytes;
        }

        public static int getInt(byte[] data, int offset, int length)
        {
            int val = 0;
            for (int i = 0; i < length; i++) val |= (data[offset + i] & 0xff) << ((length - i - 1) * 8);
            return val;
        }

        public static long getLong(byte[] data, int offset, int length)
        {
            long val = 0;
            for (int i = 0; i < length; i++) val |= ((long)data[offset + i] & 0xff) << ((length - i - 1) * 8);
            return val;
        }

        public static bool getBit(byte[] data, int pos)
        {
            return ((data[pos / 8] >> (pos % 8)) & 0x01) == 0x01;
        }

        public static byte[] concat(params byte[][] byteArrays)
        {
            int len = 0, index = 0;
            for (int i = 0; i < byteArrays.Length; i++) len += byteArrays[i].Length;
            byte[] buff = new byte[len];
            for (int i = 0; i < byteArrays.Length; i++)
            {
                Array.Copy(byteArrays[i], 0, buff, index, byteArrays[i].Length);
                index += byteArrays[i].Length;
            }
            return buff;
        }

        public static bool compare(byte[] data1, byte[] data2)
        {
            if (data1.Length != data2.Length) return false;
            for (int i = 0; i < data1.Length; i++)
                if ((data1[i] & 0xff) != (data2[i] & 0xff)) return false;
            return true;
        }
    }
}
