using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimpleProtobuf
{
    class PathManager
    {
        public static PathManager create() { return new PathManager(); }

        public void MakeProtobuf(string platform, string isMakeStringFunc, string src_path, string dest_path)
        {
            if (src_path.Contains('*'))
            {
                string[] paths = src_path.Split('\\');
                string file_name = paths[paths.Length - 1];

                string[] splits = file_name.Split('*');
                string suffix = splits[splits.Length - 1];
                Console.WriteLine(suffix);

                string dir_path = src_path.Substring(0, src_path.Length - file_name.Length);
                DirectoryInfo dir_info = new DirectoryInfo(dir_path);
                foreach (FileInfo fi in dir_info.GetFiles())
                {
                    if (fi.FullName.EndsWith(suffix))
                    {
                        Parse(platform, isMakeStringFunc, fi.FullName, dest_path);
                    }
                }
            }
            else
            {
                Parse(platform, isMakeStringFunc, src_path, dest_path);
            }
        }

        static void Parse(string platform, string isMakeStringFunc, string src_path, string dest_path)
        {
            if (platform == "cshap")
            {
                ParseCshap(bool.Parse(isMakeStringFunc), src_path, dest_path);
            }
            else if (platform == "java")
            {
                ParseJava(bool.Parse(isMakeStringFunc), src_path, dest_path);
            }
            else if (platform == "lua")
            {
                ParseLua(bool.Parse(isMakeStringFunc), src_path, dest_path);
            }
            else if (platform == "cplusplus")
            { 
                
            }
            else
            {
                throw new Exception("Please choose correct platform (java or cshap)");
            }
        }

        static void ParseCshap(bool isMakeString, string src_path, string dest_path)
        {
            ParseTree tree = new ParseTree();
            tree.ParseFile(src_path);

            CSharpDump dump = new CSharpDump();
            dump.isMakeString = isMakeString;
            dump.DumpToFile(dest_path, tree);
        }

        static void ParseJava(bool isMakeString, string src_path, string dest_path)
        {
            ParseTree tree = new ParseTree();
            tree.ParseFile(src_path);

            JavaDump jDump = new JavaDump();
            jDump.isMakeString = isMakeString;
            jDump.DumpToFile(dest_path, tree);

        }

        static void ParseLua(bool isMakeString, string src_path, string dest_path)
        {
            ParseTree tree = new ParseTree();
            tree.ParseFile(src_path);

            LuaDump dump = new LuaDump();
            dump.isMakeString = isMakeString;
            dump.DumpToFile(dest_path, tree);
        }

        static void ParseCplusplus(bool isMakeString, string src_path, string dest_path)
        {
            ParseTree tree = new ParseTree();
            tree.ParseFile(src_path);


        }
    }
}
