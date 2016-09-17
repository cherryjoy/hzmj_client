using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SimpleProtobuf
{
    public class ProtocalDeSerialize
    {
        public static ProtocalDeSerialize create(byte[] buffer) { return new ProtocalDeSerialize(buffer); }

        private int position;
        private byte[] buffer;

        private ProtocalDeSerialize(byte[] buffer)
        {
            this.buffer = buffer;
            this.position = 0;
        }

        public int DeSerializeInt32()
        {
            long v = 0;
            int offset = 0;
            while ((buffer[position] & 0x80) != 0)
            {
                v |= (long)(buffer[position++] & 0x7f) << offset;
                offset += 7;
            }
            v |= (long)(buffer[position++] & 0x7f) << offset;
            int val = (int)(v >> 1) ^ (int)(-(v & 1));
            return val;
        }

        public long DeSerializeInt64()
        {
            long v = BitConverter.ToInt64(buffer, position);
            position += Marshal.SizeOf(typeof(long));
            return v;
        }

        public float DeSerializeFloat()
        {
            int v_32 = BitConverter.ToInt32(buffer, position);
            float v = BitConverter.ToSingle(BitConverter.GetBytes(v_32), 0);
            position += Marshal.SizeOf(typeof(int));
            return v;
        }

        public string DeSerializeString()
        {
            int count = DeSerializeInt32();
            if (count <= 0)
                return null;
            string s = Encoding.UTF8.GetString(buffer, position, count);
            position += count;
            return s;
        }

        public byte DeSerializeBytes()
        {
            return buffer[position++];
        }

        public bool DeSerializeBool()
        {
            return (buffer[position++] & 0xff) != 0;
        }

        public byte[] DeSerializeOptional()
        {
            int offset = 0;
            int startPos = position;
            while ((buffer[position] & 0x80) != 0)
            {
                position++;
                offset++;
            }
            offset++;
            position++;
            byte[] bytes = new byte[offset];
            Array.Copy(buffer, startPos, bytes, 0, offset);
            return bytes;
        }

        #region DeSerialize Array
        public List<int> DeSerializeIntArray()
        {
            int len = DeSerializeInt32();
            if (len <= 0) return null;

            List<int> vals = new List<int>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeInt32());
            }
            return vals;
        }

        public List<long> DeSerializeLongArray()
        {
            int len = DeSerializeInt32();
            if (len <= 0) return null;

            List<long> vals = new List<long>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeInt64());
            }
            return vals;
        }

        public List<float> DeserializeFloatArray()
        {
            int len = DeSerializeInt32();
            if (len <= 0) return null;

            List<float> vals = new List<float>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeFloat());
            }
            return vals;
        }

        public List<string> DeSerializeStringArray()
        {
            int len = DeSerializeInt32();
            if (len <= 0) return null;
            List<string> vals = new List<string>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeString());
            }
            return vals;
        }

        public List<byte> DeSerializeByteArray()
        {
            int len = DeSerializeInt32();
            if (len <= 0) return null;
            List<byte> vals = new List<byte>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeBytes());
            }
            return vals;
        }

        public List<bool> DeSerializeBoolArray()
        {
            int len = DeSerializeInt32();
            if (len <= 0) return null;
            List<bool> vals = new List<bool>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeBool());
            }
            return vals;
        }
        #endregion

        static public string BytesToString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(ByteToString(bytes[i]));
                sb.Append(' ');
            }
            return sb.ToString();
        }

        static public string ByteToString(byte b)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 7; i >= 0; i--)
            {
                byte t = b;
                if (((t = (byte)(t >> i)) & 0x01) != 0)
                {
                    sb.Append('1');
                }
                else
                {
                    sb.Append('0');
                }
            }
            return sb.ToString();
        }
    }
}
