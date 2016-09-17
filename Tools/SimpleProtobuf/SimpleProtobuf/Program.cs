using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimpleProtobuf
{
    class Program
    {
        static void Main(string[] args)
        {
            string platform = args[0];          //platform param (java or cshap)
            string isMakeString = args[1];      //be make MakeString() function flag (true is make)
            string src_path = args[2];          //proto src path
            string dest_path = args[3];         //protobuf dest path
            PathManager.create().MakeProtobuf(platform, isMakeString, src_path, dest_path);
        }

        static void JavaTest()
        {
            string platform = "java";                                           //platform param (java or cshap)
            string isMakeString = "true";                                       //be make MakeString() function flag (true is make)
            string src_path = @"E:\ProgramArthur2\protocol\NetProtocol\*.proto";          //proto src path
            string dest_path =@"D:\";                       //protobuf dest path
            PathManager.create().MakeProtobuf(platform, isMakeString, src_path, dest_path);
        }

        static void CshapTest(){
            string platform = "cshap";                                         //platform param (java or cshap)
            string isMakeString = "true";                                       //be make MakeString() function flag (true is make)
            string src_path = @"E:\ProgramArthur2\protocol\NetProtocol\*.proto";          //proto src path
            string dest_path = @"E:\arthur2\ScriptProject\Code\Scripts\Logic\NetProtocol";                        //protobuf dest path
            PathManager.create().MakeProtobuf(platform, isMakeString, src_path, dest_path);
        }

         static void LuaTest(){
             string platform = "lua";                                         //platform param (java or cshap)
            string isMakeString = "true";                                       //be make MakeString() function flag (true is make)
            string src_path = @"E:\ProgramArthur2\protocol\NetProtocol\*.proto";          //proto src path
            string dest_path = @"E:\arthur2\Assets\Resources\LuaScripts\NetProtocol";                        //protobuf dest path
            PathManager.create().MakeProtobuf(platform, isMakeString, src_path, dest_path);
        }

         static void CplusplusTest()
         {
             string platform = "cplusplus";
             string isMakeString = "true";
             string src_path = @"E:\ProgramArthur2\protocol\NetProtocol\*.proto";
             string dest_path = @"E:\arthur2\ScriptProject\Code\Scripts\Logic\NetProtocol";
             PathManager.create().MakeProtobuf(platform, isMakeString, src_path, dest_path);
         }
    }
}
