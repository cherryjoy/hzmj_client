using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SimpleProtobuf
{
    public class ProtocalSerialize
    {
        private static readonly int MAX_BUFFER_SIZE = 200 * 1024;        
        private byte[] buffer;
        private int position;
        private int bufferSize;


        public static ProtocalSerialize create() { return new ProtocalSerialize(); }
        public static ProtocalSerialize create(byte[] buffer) { return new ProtocalSerialize(buffer); }
        public static ProtocalSerialize create(ref byte[] buffer) { return new ProtocalSerialize(ref buffer);  }

        public ProtocalSerialize()
        {
            this.buffer = new byte[4096];
            this.position = 0;
            this.bufferSize = this.buffer.Length;
        }

        public ProtocalSerialize(byte[] buffer)
        {
            this.buffer = buffer;
            this.position = 0;
            this.bufferSize = buffer.Length;
        }

        public ProtocalSerialize(ref byte[] buffer)
        {
            this.buffer = buffer;
            this.position = 0;
            this.bufferSize = buffer.Length;
        }

        private void computeBufferLimit(int data_len)
        {
            if (position + data_len > MAX_BUFFER_SIZE)
            {
                Console.WriteLine("send data size is big than " + MAX_BUFFER_SIZE);
            }
            if (position + data_len > bufferSize)
            {
                int len_2 = bufferSize * 2;
                byte[] newdata = new byte[len_2];
                Array.Copy(buffer, newdata, bufferSize);
                buffer = newdata;
                bufferSize = len_2;
            }
        }


        static private int computeRawInt32Size(int val)
        {
            if ((val & (0xffffffff << 7)) == 0) return 1;
            if ((val & (0xffffffff << 14)) == 0) return 2;
            if ((val & (0xffffffff << 21)) == 0) return 3;
            if ((val & (0xffffffff << 28)) == 0) return 4;
            return 5;
        }

        public void serializeInt32(int val)
        {
            computeBufferLimit(computeRawInt32Size(val));
            long v = (long)val;
            v = (v << 1) ^ (v >> 31);
            if (v > 0)
            {
                while (v > 0)
                {
                    byte b = (byte)(v & 0x7f);
                    v = v >> 7;
                    if (v > 0) b |= 0x80;
                    buffer[position++] = b;
                }
            }
            else
            {
                buffer[position++] = (byte)0;
            } 
        }

        public void serializeInt64(long v)
        {
            int len = Marshal.SizeOf(typeof(long));
            computeBufferLimit(len);
            for (int i = 0; i < len; i++)
            {
                buffer[position++] = (byte)(v & 0xffL);
                v = v >> 8;
            }
        }

        public void serializeFloat(float v)
        {
            int fbits = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            for (int i = 0; i < 4; i++)
            {
                buffer[position++] = (byte)(fbits & 0xff);
                fbits = fbits >> 8;
            }
        }

        public void serializeString(string v)
        {
            if (string.IsNullOrEmpty(v)) { serializeInt32(0); return; }
            byte[] bytes = Encoding.UTF8.GetBytes(v);
            int len = bytes.Length;
            serializeInt32(len);
            computeBufferLimit(len);
            Array.Copy(bytes, 0, buffer, position, len);
            position += len;
        }

        public void serializeBool(bool v)
        {
            computeBufferLimit(1);
            buffer[position++] = v ? (byte)1 : (byte)0;
        }

        public void serializeByte(byte b)
        {
            computeBufferLimit(1);
            buffer[position++] = b;
        }

        public void serializeOptional(byte[] bytes)
        {
            int len = bytes.Length;
            computeBufferLimit(len);
            for (int i = len - 1; i >= 0; i--)
            {
                if ((bytes[i] | 0x00) != 0)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        byte b = bytes[j];
                        if (i != j)
                            b |= 0x80;
                        buffer[position++] = b;
                    }
                    break;
                }
            }
        }


        #region serialize array
        public void serializeInt32Array(List<int> v)
        {
            if (v == null) { serializeInt32(0); return; }
            int len = v.Count;
            serializeInt32(len);
            for (int i = 0; i < len; i++)
            {
                serializeInt32(v[i]);
            }
        }

        public void serializeInt64Array(List<long> v)
        {
            if (v == null) { serializeInt32(0); return; }
            int len = v.Count;
            serializeInt32(len);
            for (int i = 0; i < len; i++)
            {
                serializeInt64(v[i]);
            }
        }

        public void serializeFloatArray(List<float> v)
        {
            if (v == null) { serializeInt32(0); return; }
            int len = v.Count;
            serializeInt32(len);
            for (int i = 0; i < len; i++)
            {
                serializeFloat(v[i]);
            }
        }

        public void serializeBoolArray(List<bool> v)
        {
            if (v == null) { serializeInt32(0); return; }
            int len = v.Count;
            serializeInt32(len);
            for (int i = 0; i < len; i++)
            {
                serializeBool(v[i]);
            }
        }

        public void serializeBytes(List<byte> v)
        {
            if (v == null) { serializeInt32(0); return; }
            int len = v.Count;
            for (int i = 0; i < len; i++)
            {
                serializeByte(v[i]);
            }
        }

        public void serializeStringArray(List<string> v)
        {
            if (v == null) { serializeInt32(0); return; }
            int len = v.Count;
            for (int i = 0; i < len; i++)
            {
                serializeString(v[i]);
            }
        }

        public byte[] toArray()
        {
            byte[] buf = new byte[position];
            Array.Copy(buffer, buf, position);
            return buf;
        }

        #endregion

        #region Get Optional Flag
        static public bool HasOptionalFlag(byte[] data, int idx)
        {
            bool flag = false;
            int row = idx / 7;
            if (row < data.Length)
            {
                int col = idx % 7;
                byte b = data[row];
                flag = ((b & (0x01 << col)) != 0);
            }
            return flag;
        }
        #endregion

    }
}
