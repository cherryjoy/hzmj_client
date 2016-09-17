using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SimpleProtobuf
{
    class LuaDump
    {
        public bool isMakeString = false;
        public void DumpToFile(string pathname, ParseTree tree)
        {
            for (int i = 0; i < tree.import_proto_.Count; i++)
            {
                string file_name = Path.GetFileNameWithoutExtension(tree.import_proto_[i].file_path_);
                DumpToFile(pathname, tree.import_proto_[i]);
            }

            if (Directory.Exists(pathname) == false)
            {
                Directory.CreateDirectory(pathname);
            }

            StreamWriter sw = new StreamWriter(Path.Combine(pathname, Path.GetFileNameWithoutExtension(tree.file_path_)) + ".txt");
            sw.Write(Dump(tree));
            sw.Close();
        }

        public string Dump(ParseTree tree)
        {
            string out_str = string.Empty;

            foreach (EnumNode enum_node in tree.enums_.Values)
                out_str += DumpEnum(enum_node);
            for (int i = 0; i < tree.nodes_.Count; i++)
            {
                // dump class
                out_str += DumpClass(tree.nodes_[i]);
            }

            return out_str;
        }

        string DumpEnum(EnumNode enum_node /*Dictionary<string, Dictionary<string, object>> enums */)
        {
            string out_str = string.Empty;
            foreach (var v in enum_node.enums_)
            {
                out_str += "enum" + v.Key +" = {"+ Environment.NewLine;
                Dictionary<string, object> items = v.Value;

                foreach (var i in items)
                {
                    out_str += "\t" + i.Key + " = " + i.Value.ToString() + "," + Environment.NewLine;

                }
                out_str += "}" + Environment.NewLine + Environment.NewLine;
            }
            return out_str;
        }

        string DumpClass(MessageNode msg_node)
        {
            string out_str = string.Empty;
            int count = 0;

            foreach (ValueNode node in msg_node.nodes)
            {
                if (node.type == Token.eKeyWord.e_optional)
                    count++;
            }

            out_str += "-- " + msg_node.name + Environment.NewLine;
            out_str += msg_node.name + " = {}" + Environment.NewLine;
            out_str += msg_node.name + ".__index = "+ msg_node.name + Environment.NewLine;

            //dump create function
            out_str += "function "+msg_node.name + ".Create()" + Environment.NewLine;
            out_str += "\tlocal obj = setmetatable({},"+msg_node.name+")"+ Environment.NewLine;
            out_str += "\treturn obj" + Environment.NewLine;
            out_str += "end" + Environment.NewLine;

            out_str += Environment.NewLine;

            out_str += LuaDumpDeSerialize.create().MakeDeserialize(msg_node);

            out_str += LuaDumpSerialize.create().MakeSerialize(msg_node,count);

            out_str += MakeGetSize(msg_node);

            if (isMakeString)
                out_str += DumpMakeString(msg_node) + Environment.NewLine;

            out_str += Environment.NewLine;
            return out_str;
        }

        string MakeGetSize(MessageNode node)
        {
            string out_str = string.Empty;
            out_str += Environment.NewLine;
            out_str += "function "+node.name+".GetSize(obj)" + Environment.NewLine;
            out_str += "\tlocal size = 0" + Environment.NewLine;

            int optionalCount = 0;

            int sizeWeKonw = sizeof(int) + 1; //fieldCount is always a int and always exist

            for (int i = 0; i < node.nodes.Count; i++)
            {
                if (node.nodes[i].type == Token.eKeyWord.e_required)
                {
                    if (node.nodes[i].value_type == Token.eValueType.e_bool)
                    {
                        sizeWeKonw += 1;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_ref)
                    {
                        sizeWeKonw += sizeof(int) + 1;//datalen
                        out_str += "\tif (obj." + node.nodes[i].name + " ~= nil) then" + Environment.NewLine;
                        out_str += "\t\tsize = size + obj." + node.nodes[i].name + ":GetSize();" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;

                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_enum || node.nodes[i].value_type == Token.eValueType.e_fixed32 || node.nodes[i].value_type == Token.eValueType.e_int32 || node.nodes[i].value_type == Token.eValueType.e_uint32)
                    {
                        sizeWeKonw += sizeof(int)+1;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_fixed64 || node.nodes[i].value_type == Token.eValueType.e_int64 || node.nodes[i].value_type == Token.eValueType.e_uint64 || node.nodes[i].value_type == Token.eValueType.e_longid)
                    {
                        sizeWeKonw += sizeof(long)+1;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_float)
                    {
                        sizeWeKonw += sizeof(float)+1;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_string)
                    {
                        out_str += "\tlocal sizeStr = string.len(obj."+node.nodes[i].name+")" + Environment.NewLine;
                        out_str += "\tsize = size + serialize_get_str_actual_size(sizeStr)" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_byte) {
                        out_str += "\tsize = size + obj."+node.nodes[i].name+"Length" + Environment.NewLine;
                    }
                }
                else if (node.nodes[i].type == Token.eKeyWord.e_repeated)
                {
                    sizeWeKonw += sizeof(int) + 1;//array count
                    if (node.nodes[i].value_type == Token.eValueType.e_bool)
                    {
                        out_str += "\tif (obj." + node.nodes[i].name + " ~= nil) then" + Environment.NewLine;
                        out_str += "\t\tfor i=1,#(obj." + node.nodes[i].name + ") do" + Environment.NewLine;
                        out_str += "\t\t\tsize = size + 1" + Environment.NewLine;
                        out_str += "\t\tend" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_ref)
                    {
                        out_str += "\tif (obj." + node.nodes[i].name + " ~= nil) then" + Environment.NewLine;
                        out_str += "\t\tfor i=1,#(obj." + node.nodes[i].name + ") do" + Environment.NewLine;

                        out_str += "\t\t\tsize = size + " + sizeof(int) + 1 + Environment.NewLine;//datalen
                        out_str += "\t\t\tsize = size + (obj." + node.nodes[i].name + "[i]):GetSize()" + Environment.NewLine;
                        out_str += "\t\tend" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_enum || node.nodes[i].value_type == Token.eValueType.e_fixed32 || node.nodes[i].value_type == Token.eValueType.e_int32 || node.nodes[i].value_type == Token.eValueType.e_uint32)
                    {
                        out_str += "\tif (obj." + node.nodes[i].name + " ~= nil) then" + Environment.NewLine;
                        out_str += "\t\tfor i=1,#(obj." + node.nodes[i].name + ") do" + Environment.NewLine;
                        out_str += "\t\t\tsize = size + " + sizeof(int) + Environment.NewLine;
                        out_str += "\t\tend" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;

                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_fixed64 || node.nodes[i].value_type == Token.eValueType.e_int64 || node.nodes[i].value_type == Token.eValueType.e_uint64 || node.nodes[i].value_type == Token.eValueType.e_longid)
                    {
                        out_str += "\tif (obj." + node.nodes[i].name + " ~= nil) then" + Environment.NewLine;
                        out_str += "\t\tfor i=1,#(obj." + node.nodes[i].name + ") do" + Environment.NewLine;
                        out_str += "\t\t\tsize = size + " + sizeof(long) + Environment.NewLine;
                        out_str += "\t\tend" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_float)
                    {
                        out_str += "\tif (obj." + node.nodes[i].name + " ~= nil) then" + Environment.NewLine;
                        out_str += "\t\tfor i=1,#(obj." + node.nodes[i].name + ") do" + Environment.NewLine;
                        out_str += "\t\t\tsize = size + " + sizeof(float) + Environment.NewLine;
                        out_str += "\t\tend" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_string)
                    {
                        out_str += "\tif (obj." + node.nodes[i].name + " ~= nil) then" + Environment.NewLine;
                        out_str += "\t\tfor i=1,#(obj." + node.nodes[i].name + ") do" + Environment.NewLine;

                        out_str += "\t\t\tlocal sizeStr = string.len(obj." + node.nodes[i].name + "[i])" + Environment.NewLine;
                        out_str += "\t\t\tsize = size + serialize_get_str_actual_size(sizeStr)" + Environment.NewLine;

                        out_str += "\t\tend" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;
                    }
                }
                else if (node.nodes[i].type == Token.eKeyWord.e_optional)
                {
                    optionalCount++;
                    out_str += "\tif (obj." + node.nodes[i].name + " ~= nil) then" + Environment.NewLine;
                    if (node.nodes[i].value_type == Token.eValueType.e_bool)
                    {
                        out_str += "\t\tsize = size + 1" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_ref)
                    {
                        sizeWeKonw += sizeof(int) + 1;//datalen
                        out_str += "\t\tsize = size + obj." + node.nodes[i].name + ":GetSize()" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_enum || node.nodes[i].value_type == Token.eValueType.e_fixed32 || node.nodes[i].value_type == Token.eValueType.e_int32 || node.nodes[i].value_type == Token.eValueType.e_uint32)
                    {
                        out_str += "\t\tsize = size + " + sizeof(int) + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_fixed64 || node.nodes[i].value_type == Token.eValueType.e_int64 || node.nodes[i].value_type == Token.eValueType.e_uint64 || node.nodes[i].value_type == Token.eValueType.e_longid)
                    {
                        out_str += "\t\tsize = size + " + sizeof(long) + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_float)
                    {
                        out_str += "\t\tsize = size + " + sizeof(float) + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_string)
                    {
                        out_str += "\t\tlocal sizeStr = string.len(obj." + node.nodes[i].name + ")" + Environment.NewLine;
                        out_str += "\t\tsize = size + serialize_get_str_actual_size(sizeStr)" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_byte)
                    {
                        out_str += "\tsize = size + obj." + node.nodes[i].name + "Length" + Environment.NewLine;
                    }

                    out_str += "\tend" + Environment.NewLine;
                }
            }
            sizeWeKonw += (int) Math.Ceiling(optionalCount / 7.0);
            if(sizeWeKonw>0)
                out_str += "\tsize = size + "+sizeWeKonw + Environment.NewLine;
            out_str += "\treturn size" + Environment.NewLine;
            out_str += "end" + Environment.NewLine;

            return out_str;
        }

        #region Make ToString
        string DumpMakeString(MessageNode node)
        {
            string out_str = string.Empty;
            out_str += Environment.NewLine;
            out_str += "function "+node.name + ".MakeString(obj)" + Environment.NewLine;
            out_str += "\tout_str = \"\"" + Environment.NewLine;
            for (int i = 0; i < node.nodes.Count; i++)
            {
                if (node.nodes[i].type == Token.eKeyWord.e_required || node.nodes[i].type == Token.eKeyWord.e_optional)
                {
                    if (node.nodes[i].value_type == Token.eValueType.e_ref)
                    {
                        out_str += "\tout_str = out_str .. \"\\nobj." + node.nodes[i].name + " = {\\n\";" + Environment.NewLine;
                        out_str += "\tif (obj." + node.nodes[i].name + " ~=nil) then out_str = out_str .. obj." + node.nodes[i].name + ":MakeString() end" + Environment.NewLine;
                        out_str += "\tout_str = out_str .. \"}\\n\\n\";" + Environment.NewLine;
                    }
                    else
                    {
                        out_str += MakeCommonString(node.nodes[i]);
                    }
                }
                else if (node.nodes[i].type == Token.eKeyWord.e_repeated)
                {
                    if (node.nodes[i].value_type == Token.eValueType.e_ref)
                    {
                        out_str += "\tif (obj." + node.nodes[i].name + " ~=nil) then " + Environment.NewLine;
                        out_str += "\t\tfor i=1, #(obj." + node.nodes[i].name + ") do" + Environment.NewLine;
                        out_str += "\t\t\tout_str = out_str .. \"obj." + node.nodes[i].name + "[\" .. i .. \"]\\n{\\n\" .. string.format(\"\\t%s\", obj." + node.nodes[i].name + "[i]:MakeString()) .. \"}\\n\" end" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_enum)
                    {

                        out_str += "\tif (obj." + node.nodes[i].name + " ~=nil) then " + Environment.NewLine;
                        out_str += "\t\tfor i=1, #(obj." + node.nodes[i].name + ") do" + Environment.NewLine;
                        out_str += "\t\t\tout_str = out_str .. \"obj." + node.nodes[i].name + "[\" .. i .. \"]\\n{\\n\\t\" .. obj." + node.nodes[i].name + "[i] .. \"\\n}\\n\" end" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;

                    }
                    else
                    {

                        out_str += "\tif (obj." + node.nodes[i].name + " ~=nil) then " + Environment.NewLine;
                        out_str += "\t\tfor i=1, #(obj." + node.nodes[i].name + ") do" + Environment.NewLine;
                        out_str += "\t\t\tout_str = out_str .. \"obj." + node.nodes[i].name + "[\" .. i .. \"]\\n{\\n\" .. string.format(\"\\t%s\",tostring(obj." + node.nodes[i].name + "[i])) .. \"\\n}\\n\" end" + Environment.NewLine;
                        out_str += "\tend" + Environment.NewLine;
                    }
                }
            }
            out_str += "\treturn out_str" + Environment.NewLine;
            out_str += "end" + Environment.NewLine;
            return out_str;
        }

        string MakeCommonString(ValueNode node)
        {
            string out_str = string.Empty;
            out_str += "\t\t\tout_str = out_str .. \"" + node.name + " = \" .. string.format(\"%s\\n\",tostring(obj." + node.name + "))" + Environment.NewLine;
            return out_str;
        }

        #endregion
    }
}
