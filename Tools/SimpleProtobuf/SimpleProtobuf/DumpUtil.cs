using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

namespace SimpleProtobuf
{
    class DumpUtil
    {
        private static readonly int MAX_BUFFER_SIZE = 200 * 1024;
        static private void ComputeDataLimit(ref byte[] data, int idx, int data_len)
        {
            if (idx + data_len >= MAX_BUFFER_SIZE)
            {
                Console.WriteLine("send data size is big than " + MAX_BUFFER_SIZE);
            }

            if (idx + data_len >= data.Length)
            {
                int len_2 = data.Length * 2;
                byte[] newdata = new byte[len_2];
                Array.Copy(data, newdata, data.Length);
                data = newdata;
            }
        }


        static private int computeIntLength(int val)
        {
            if ((val & (0xffffffff << 7)) == 0) return 1;
            if ((val & (0xffffffff << 14)) == 0) return 2;
            if ((val & (0xffffffff << 21)) == 0) return 3;
            if ((val & (0xffffffff << 28)) == 0) return 4;
            return 5;
        }

        #region SerializeInt
        public static void SerializeInt(ref byte[] data, ref int idx, int val)
        {
            ComputeDataLimit(ref data, idx, computeIntLength(val));
            long v = (long)val;
            v = (v << 1) ^ (v >> 31);
            if (v > 0)
            {
                while (v > 0)
                {
                    byte b = (byte)(v & 0x7f);
                    v = v >> 7;
                    if (v > 0) b |= 0x80;
                    data[idx++] = b;
                }
            }
            else
            {
                data[idx++] = (byte)0;
            }    
        }

        public static int DeSerializeInt(byte[] data, ref int idx)
        {
            long v = 0;
            int offset = 0;
            while((data[idx] & 0x80) != 0){
                v |= (long)(data[idx++] & 0x7f) << offset;
                offset += 7;
            }
            v |= (long)(data[idx++] & 0x7f) << offset;       
            int val = (int)(v >> 1) ^ (int)(-(v & 1));
            return val;
        }
        #endregion

        #region SerializeLong
        public static void SerializeLong(ref byte[] data, ref int idx, long v)
        {
            int len = Marshal.SizeOf(typeof(long));
            ComputeDataLimit(ref data, idx, len);
            for (int i = 0; i < len; i++)
            {
                data[idx + i] = (byte)(v & 0xffL);
                v = v >> 8;
            }
            idx += len;
        }

        public static long DeSerializeLong(byte[] data, ref int idx)
        {
            long v = BitConverter.ToInt64(data, idx);
            idx += Marshal.SizeOf(typeof(long));
            return v;
        }
        #endregion

        #region SerializeFloat
        public static void SerializeFloat(ref byte[] data, ref int idx, float v)
        {
            byte[] bytes = BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(v), 0));
            int length = bytes.Length;
            ComputeDataLimit(ref data, idx, length);
            Array.Copy(bytes, 0, data, idx, length);
            idx += length;
        }

        public static float DeSerializeFloat(byte[] data, ref int idx)
        {
            int v_32 = BitConverter.ToInt32(data, idx);
            float v = BitConverter.ToSingle(BitConverter.GetBytes(v_32), 0);
            idx += Marshal.SizeOf(typeof(int));
            return v;
        }
        #endregion
        
        #region SerializeDouble
        public static void SerializeDouble(ref byte[] data, ref int idx, double v)
        {
            long l = BitConverter.ToInt64(BitConverter.GetBytes(v), 0);
            SerializeLong(ref data, ref idx, l);
        }

        public static double DeSeralizeDouble(byte[] data, ref int idx)
        {
            long l = DeSerializeLong(data, ref idx);
            double v = BitConverter.ToDouble(BitConverter.GetBytes(l), 0);
            return v;
        }
        #endregion
        
        #region SerializeString
        public static void SerializeString(ref byte[] data, ref int idx, string v)
        {
            if (v == null) { SerializeInt(ref data, ref idx, 0); return; }
            byte[] bytes = Encoding.UTF8.GetBytes(v);
            int len = bytes.Length;
            SerializeInt(ref data, ref idx, len);
            ComputeDataLimit(ref data, idx, len);
            Array.Copy(bytes, 0, data, idx, len);
            idx += len;
        }
        // length
        public static string DeSerializeString(byte[] data, ref int idx)
        {
            int count = DeSerializeInt(data, ref idx);
            if (count <= 0)
                return null;
            string s = Encoding.UTF8.GetString(data, idx, count);
            idx += count;
            return s;
        }
        #endregion
        
        #region SerializeBytes
        public static void SerializeBytes(ref byte[] data, ref int index, byte b)
        {
            ComputeDataLimit(ref data, index, 1);
            data[index++] = b;
        }

        public static byte DeSerializeBytes(byte[] data, ref int idx)
        {
            return data[idx++];
        }
        #endregion
        
        #region SerializeBool
        public static void SerializeBool(ref byte[] data, ref int index, bool v)
        {
            ComputeDataLimit(ref data, index, 1);
            data[index++] = v ? (byte)1 : (byte)0;
        }

        public static bool DeSerializeBool(byte[] data, ref int index)
        {
            return (data[index++] & 0xff) != 0;
        }
        #endregion  
        
        #region SerializeOptional
        public static void SerializeOptional(ref byte[] data, ref int idx, byte[] flags_)
        {
            int count = flags_.Length;
            ComputeDataLimit(ref data, idx, count);
            for (int i = count - 1; i >= 0; i--)
            {
                if ((flags_[i] | 0x00) != 0)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        byte b = flags_[j];
                        if (i != j)
                            b |= 0x80;
                        data[idx + j] = b;
                    }
                    idx += i;
                    break;
                }
            }
            idx++;
        }

        public static byte[] DeSerializeOptional(byte[] data, ref int idx)
        {
            int offset = 0;
            while ((data[idx] & 0x80) != 0)
            {
                idx++;
                offset++;
            }
            offset++;
            idx++;
            byte[] bytes = new byte[offset];
            Array.Copy(data, bytes, offset);
            return bytes;
        }
        #endregion

        #region Serialize Array
        static public void SerializeIntArray(ref byte[] data, ref int idx, List<int> v)
        {
            if (v == null) { SerializeInt(ref data, ref idx, 0); return; }
            int len = v.Count;
            SerializeInt(ref data, ref idx, len);
            for (int i = 0; i < len; i++)
            {
                SerializeInt(ref data, ref idx, v[i]);
            }
        }

        static public void SerializeLongArray(ref byte[] data, ref int idx, List<long> v)
        {
            if (v == null) { SerializeInt(ref data, ref idx, 0); return; }
            int len = v.Count;
            SerializeInt(ref data, ref idx, len);
            for (int i = 0; i < len; i++ )
            {
                SerializeLong(ref data, ref idx, v[i]);
            }
        }

        static public void SerializeFloatArray(ref byte[] data, ref int idx, List<float> v)
        {
            if (v == null) { SerializeInt(ref data, ref idx, 0); return; }
            int len = v.Count;
            SerializeInt(ref data, ref idx, len);
            for (int i = 0; i < len; i++)
            {
                SerializeFloat(ref data, ref idx, v[i]);
            }
        }

        static public void SerializeDoubleArray(ref byte[] data, ref int idx, List<double> v)
        {
            if (v == null) { SerializeInt(ref data, ref idx, 0); return; }
            int len = v.Count;
            SerializeInt(ref data, ref idx, len);
            for (int i = 0; i < len; i++)
            {
                SerializeDouble(ref data, ref idx, v[i]);
            }
        }

        static public void SerializeStringArray(ref byte[] data, ref int idx, List<string> v)
        {
            if (v == null) { SerializeInt(ref data, ref idx, 0); return; }
            int len = v.Count;
            SerializeInt(ref data, ref idx, len);
            for (int i = 0; i < len; i++){
                SerializeString(ref data, ref idx, v[i]);
            }
        }

        static public void SerializeByteArray(ref byte[] data, ref int idx, List<byte> v)
        {
            if (v == null) { SerializeInt(ref data, ref idx, 0); return; }
            int len = v.Count;
            SerializeInt(ref data, ref idx, len);
            for (int i = 0; i < len; i++)
            {
                SerializeBytes(ref data, ref idx, v[i]);
            }
        }

        static public void SerializeBoolArray(ref byte[] data, ref int idx, List<bool> v)
        {
            if (v == null) { SerializeInt(ref data, ref idx, 0); return; }
            int len = v.Count;
            SerializeInt(ref data, ref idx, len);
            for (int i = 0; i < len; i++)
            {
                SerializeBool(ref data, ref idx, v[i]);
            }
        }

        #endregion

        #region DeSerialize Array
        public static List<int> DeSerializeIntArray(byte[] data, ref int idx)
        {
            int len = DeSerializeInt(data, ref idx);
            if (len <= 0) return null;

            List<int> vals = new List<int>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeInt(data, ref idx));
            }
            return vals;
        }

        static public List<long> DeSerializeLongArray(byte[] data, ref int idx)
        {
            int len = DeSerializeInt(data, ref idx);
            if (len <= 0) return null;

            List<long> vals = new List<long>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeLong(data, ref idx));
            }
            return vals;
        }

        public static List<float> DeserializeFloatArray(byte[] data, ref int idx)
        {
            int len = DeSerializeInt(data, ref idx);
            if (len <= 0) return null;

            List<float> vals = new List<float>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeFloat(data, ref idx));
            }
            return vals;
        }

        public static List<double> DeSerializeDoubleArray(byte[] data, ref int idx)
        {
            int len = DeSerializeInt(data, ref idx);
            if (len <= 0) return null;

            List<double> vals = new List<double>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSeralizeDouble(data, ref idx));
            }
            return vals;
        }

        public static List<string> DeSerializeStringArray(byte[] data, ref int idx)
        {
            int len = DeSerializeInt(data, ref idx);
            if (len <= 0) return null;
            List<string> vals = new List<string>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeString(data, ref idx));
            }
            return vals;
        }

        public static List<byte> DeSerializeByteArray(byte[] data, ref int idx)
        {
            int len = DeSerializeInt(data, ref idx);
            if (len <= 0) return null;
            List<byte> vals = new List<byte>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeBytes(data, ref idx));
            }
            return vals;
        }

        public static List<bool> DeSerializeBoolArray(byte[] data, ref int idx)
        {
            int len = DeSerializeInt(data, ref idx);
            if (len <= 0) return null;
            List<bool> vals = new List<bool>();
            for (int i = 0; i < len; i++)
            {
                vals.Add(DeSerializeBool(data, ref idx));
            }
            return vals;
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


        #region Console log
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
        #endregion
    }
}
