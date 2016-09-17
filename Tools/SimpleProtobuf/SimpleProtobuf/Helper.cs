using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimpleProtobuf
{
    class Helper
    {
        private readonly string file_path = @"D:\x-svn\Demo\log.txt";
        private FileStream stream;
        private static Helper instance_ = null;
        public static Helper newInstance()
        {
            if (instance_ == null)
            {
                instance_ = new Helper();
            }
            return instance_;
        }

        public Helper()
        {
            stream = new FileStream(file_path, FileMode.Create);   
        }

        public void WriteLine(string val)
        {
            Write(val + "\n");
        }

        public void Write(string val)
        {
            byte[] data = Encoding.Default.GetBytes(val);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public void Close() {
            stream.Close();    
        }

    }
}
