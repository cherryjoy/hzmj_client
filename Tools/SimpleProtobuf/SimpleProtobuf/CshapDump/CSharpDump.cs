using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SimpleProtobuf
{
    class CSharpDump
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

            StreamWriter sw = new StreamWriter(Path.Combine(pathname, Path.GetFileNameWithoutExtension(tree.file_path_)) + ".cs");
            sw.Write(Dump(tree));
            sw.Close();
        }

        public string Dump(ParseTree tree)
        {
            string out_str = string.Empty;
            // dump namespace
            out_str += "using SimpleProtobuf;" + Environment.NewLine;
            out_str += "using System;" + Environment.NewLine;
            out_str += "using System.Collections.Generic;" + Environment.NewLine + Environment.NewLine;
            out_str += "namespace " + tree.package_name_ + Environment.NewLine;
            out_str += "{" + Environment.NewLine;

            foreach (EnumNode enum_node in tree.enums_.Values)
                out_str += DumpEnum(enum_node);
            for (int i = 0; i < tree.nodes_.Count; i++)
            {
                // dump class
                out_str += DumpClass(tree.nodes_[i]);
            }

            out_str += "}" + Environment.NewLine;
            return out_str;
        }

        string DumpEnum(EnumNode enum_node /*Dictionary<string, Dictionary<string, object>> enums */)
        {
            string out_str = string.Empty;
            foreach (var v in enum_node.enums_)
            {
                out_str += "\tpublic enum " + v.Key + Environment.NewLine;
                out_str += "\t{" + Environment.NewLine;
                Dictionary<string, object> items = v.Value;

                foreach (var i in items)
                {
                    out_str += "\t\t" + i.Key + " = " + i.Value.ToString() + "," + Environment.NewLine;

                }
                out_str += "\t}" + Environment.NewLine + Environment.NewLine;
            }
            return out_str;
        }

        string DumpClass(MessageNode msg_node)
        {
            string out_str = string.Empty;
            int count = 0;
            out_str += "\tpublic unsafe class " + msg_node.name + Environment.NewLine;
            out_str += "\t{" + Environment.NewLine;

            string temp = string.Empty;
            int msg_node_count = msg_node.nodes.Count;
 
            for (int i = 0; i < msg_node_count; i++)
            {
                temp += DumpValue(msg_node.nodes[i], ref count);
            }
            if (count > 0)
                out_str += "\t\tprivate byte[] flags_ = new byte[" + (int)Math.Ceiling(count / 7.0f) + "];" + Environment.NewLine;
            out_str += temp;
            out_str += Environment.NewLine;

            out_str += CshapDumpDeSerialize.create().MakeDeserialize(msg_node);

            out_str += CshapDumpSerialize.create().MakeSerialize(msg_node, (int)Math.Ceiling(count / 7.0f));

            out_str += MakeGetSize(msg_node);

            if (isMakeString)
                out_str += DumpMakeString(msg_node) + Environment.NewLine;

            out_str += "\t}" + Environment.NewLine + Environment.NewLine;
            return out_str;
        }

        string DumpValue(ValueNode node, ref int count)
        {
            string out_str = string.Empty;
            if (node.type == Token.eKeyWord.e_repeated)
            {
                if (node.value_type == Token.eValueType.e_int32 ||
                    node.value_type == Token.eValueType.e_uint32 || 
                    node.value_type == Token.eValueType.e_fixed32)
                {
                    out_str += "\t\tpublic List<int> " + node.name + " = new List<int>();" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_float)
                {
                    out_str += "\t\tpublic List<float> " + node.name + " = new List<float>();" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_int64||
                    node.value_type == Token.eValueType.e_uint64 || 
                    node.value_type == Token.eValueType.e_fixed64)
                {
                    out_str += "\t\tpublic List<long> " + node.name + " = new List<long>();" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_string)
                {
                    out_str += "\t\tpublic List<string> " + node.name + " = new List<string>();" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_bool)
                {
                    out_str += "\t\tpublic List<bool> " + node.name + " = new List<bool>();" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_byte)
                {
                    out_str += "\t\tpublic List<byte> " + node.name + " = new List<byte>();" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_ref)
                {
                    out_str += "\t\tpublic List<" + node.ref_name + "> " + node.name + " = new List<" + node.ref_name + ">();" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_enum)
                {
                    out_str += "\t\tpublic List<" + node.ref_name + "> " + node.name + " = new List<" + node.ref_name + ">();" + Environment.NewLine;
                }
            }
            else if (node.type == Token.eKeyWord.e_optional)
            {
                if (node.value_type == Token.eValueType.e_int32 ||
                    node.value_type == Token.eValueType.e_uint32 || 
                    node.value_type == Token.eValueType.e_fixed32)
                {
                    out_str += "\t\tprivate int " + node.name + "_ " + DumpDefaultValue(node) + ";" + Environment.NewLine;
                    out_str += "\t\tpublic int " + node.name + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_float)
                {
                    out_str += "\t\tprivate float " + node.name + "_" + DumpDefaultValue(node) + ";" + Environment.NewLine;
                    out_str += "\t\tpublic float " + node.name + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_int64 ||
                    node.value_type == Token.eValueType.e_uint64 || 
                    node.value_type == Token.eValueType.e_fixed64)
                {
                    out_str += "\t\tprivate long " + node.name + "_" + DumpDefaultValue(node) + ";" + Environment.NewLine;
                    out_str += "\t\tpublic long " + node.name + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_string)
                {
                    out_str += "\t\tprivate string " + node.name + "_" + DumpDefaultValue(node) + ";" + Environment.NewLine;
                    out_str += "\t\tpublic string " + node.name + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_bool)
                {
                    out_str += "\t\tprivate bool " + node.name + "_" + DumpDefaultValue(node) + ";" + Environment.NewLine;
                    out_str += "\t\tpublic bool " + node.name + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_byte)
                {
                    out_str += "\t\tprivate byte " + node.name + "_" + DumpDefaultValue(node) + ";" + Environment.NewLine;
                    out_str += "\t\tpublic byte " + node.name + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_ref)
                {
                    out_str += "\t\tprivate " + node.ref_name + " " + node.name + "_" + DumpDefaultValue(node) + ";" + Environment.NewLine;
                    out_str += "\t\tpublic " + node.ref_name + " " + node.name + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_enum)
                {
                    out_str += "\t\tprivate " + node.ref_name + " " + node.name + "_" + DumpDefaultValue(node) + ";" + Environment.NewLine;
                    out_str += "\t\tpublic " + node.ref_name + " " + node.name + Environment.NewLine;
                }
                out_str += "\t\t{" + Environment.NewLine;
                out_str += "\t\t\tget{ return " + node.name + "_; }" + Environment.NewLine;
                out_str += "\t\t\tset" + Environment.NewLine + "\t\t\t{" + Environment.NewLine;
                out_str += "\t\t\t\t" + node.name + "_ = value; " + Environment.NewLine;
                if (count % 7 == 0)
                    out_str += "\t\t\t\tbyte b = flags_[" + (count / 7) + "];  b |= 0x01; flags_[" + (count++ / 7) + "] = b;" + Environment.NewLine;
                else
                    out_str += "\t\t\t\tbyte b = flags_[" + (count / 7) + "];  b |= 0x01 << " + (count % 7) + "; flags_[" + (count++ / 7) + "] = b;" + Environment.NewLine;
                out_str += "\t\t\t}" + Environment.NewLine;
                out_str += "\t\t}" + Environment.NewLine;
            }
            else
            {
                if (node.value_type == Token.eValueType.e_int32 ||
                    node.value_type == Token.eValueType.e_uint32 || 
                    node.value_type == Token.eValueType.e_fixed32)
                {
                    out_str += "\t\tpublic int " + node.name + DumpDefaultValue(node) + ";" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_float)
                {
                    out_str += "\t\tpublic float " + node.name + DumpDefaultValue(node) + ";" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_int64 ||
                    node.value_type == Token.eValueType.e_uint64 || 
                    node.value_type == Token.eValueType.e_fixed64)
                {
                    out_str += "\t\tpublic long " + node.name + DumpDefaultValue(node) + ";" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_string)
                {
                    out_str += "\t\tpublic string " + node.name + DumpDefaultValue(node) + ";" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_bool)
                {
                    out_str += "\t\tpublic bool " + node.name + DumpDefaultValue(node) + ";" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_byte)
                {
                    out_str += "\t\tpublic byte " + node.name + DumpDefaultValue(node) + ";" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_ref)
                {
                    out_str += "\t\tpublic " + node.ref_name + " " + node.name + DumpDefaultValue(node) + ";" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_enum)
                {
                    out_str += "\t\tpublic " + node.ref_name + " " + node.name + DumpDefaultValue(node) + ";" + Environment.NewLine;
                }
            }

            return out_str;
        }

        private string MakeHasValue(ValueNode node, int idx)
        {
            string out_str = string.Empty;
            out_str += "\t\tprivate bool has_" + node.name + "()";
            out_str += "{ ";
            out_str += "if ((bitField" + (int)(idx / 32.0f) + "_ & " + string.Format("0x{0:X8}", (int)Math.Pow(2, idx % 32)) + ") == " + string.Format("0x{0:X8}", (int)Math.Pow(2, idx % 32)) + ")";
            out_str += "{ ";
            out_str += "return true;";
            out_str += "} ";
            out_str += "return false;";
            out_str += "}" + Environment.NewLine;
            return out_str;
        }

        string DumpDefaultValue(ValueNode node)
        {
            if (node.defult_value != null)
                return " = " + node.defult_value.ToString();
            else if (node.type == Token.eKeyWord.e_optional)
            {
                string t_str = string.Empty;
                if (node.value_type == Token.eValueType.e_string)
                {
                    t_str += " = string.Empty";
                }
                else if (node.value_type == Token.eValueType.e_ref)
                {
                    t_str += " = null";
                }
                else if (node.value_type == Token.eValueType.e_enum)
                {

                    t_str += " = " + "("+ node.ref_name +")Enum.GetValues(typeof(" + node.ref_name + ")).GetValue(0)";
                }
                else
                {
                    t_str = " = default(";
                    if (node.value_type == Token.eValueType.e_int32 ||
                        node.value_type == Token.eValueType.e_uint32 || 
                        node.value_type == Token.eValueType.e_fixed32)
                    {
                        t_str += "int";
                    }
                    else if (node.value_type == Token.eValueType.e_float)
                    {
                        t_str += "float";
                    }
                    else if (node.value_type == Token.eValueType.e_int64 || 
                        node.value_type == Token.eValueType.e_uint64 || 
                        node.value_type == Token.eValueType.e_fixed64)
                    {
                        t_str += "long";
                    }
                    else if (node.value_type == Token.eValueType.e_bool)
                    {
                        t_str += "bool";
                    }
                    else if (node.value_type == Token.eValueType.e_byte)
                    {
                        t_str += "byte";
                    }
                    t_str += ")";
                }
                return t_str;
            }
            else
                return string.Empty;
        }


        string MakeGetSize(MessageNode node)
        {
            string out_str = string.Empty;
            out_str += Environment.NewLine;
            out_str += "\t\tpublic int GetSize(){" + Environment.NewLine;

            bool haveString = false;
            for (int i = 0; i < node.nodes.Count; i++)
            {
                if (node.nodes[i].value_type == Token.eValueType.e_string)
                    haveString = true;
            }

            int sizeWeKonw = 0;
            out_str += "\t\t\tint size = 0;" + Environment.NewLine;

            for (int i = 0; i < node.nodes.Count; i++)
            {
                if (node.nodes[i].type == Token.eKeyWord.e_required || node.nodes[i].type == Token.eKeyWord.e_optional)
                {
                    if (node.nodes[i].value_type == Token.eValueType.e_byte || node.nodes[i].value_type == Token.eValueType.e_bool)
                    {
                        sizeWeKonw += 1;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_ref)
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) size += " + node.nodes[i].name + ".GetSize();" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_enum || node.nodes[i].value_type == Token.eValueType.e_fixed32 || node.nodes[i].value_type == Token.eValueType.e_int32 || node.nodes[i].value_type == Token.eValueType.e_uint32)
                    {
                        sizeWeKonw += sizeof(int);
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_fixed64 || node.nodes[i].value_type == Token.eValueType.e_int64 || node.nodes[i].value_type == Token.eValueType.e_uint64)
                    {
                        sizeWeKonw += sizeof(long);
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_float)
                    {
                        sizeWeKonw += sizeof(float);
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_string)
                    {
                        out_str += "\t\t\tbyte[] " + node.nodes[i].name + "_b = System.Text.Encoding.UTF8.GetBytes(" + node.nodes[i].name + ");" + Environment.NewLine;
                        out_str += "\t\t\tsize += GetStrActualSize(" + node.nodes[i].name + "_b.Length);" + Environment.NewLine;
                    }
                }
                else if (node.nodes[i].type == Token.eKeyWord.e_repeated)
                {
                    if (node.nodes[i].value_type == Token.eValueType.e_byte || node.nodes[i].value_type == Token.eValueType.e_bool)
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) {" + Environment.NewLine;
                        out_str += "\t\t\t\t\tsize += 1;" + Environment.NewLine;
                        out_str += "\t\t\t\t}" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_ref)
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) {" + Environment.NewLine;
                        out_str += "\t\t\t\t\tif (" + node.nodes[i].name + "[i] != null) size += " + node.nodes[i].name + "[i].GetSize();" + Environment.NewLine;
                        out_str += "\t\t\t\t}" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_enum || node.nodes[i].value_type == Token.eValueType.e_fixed32 || node.nodes[i].value_type == Token.eValueType.e_int32 || node.nodes[i].value_type == Token.eValueType.e_uint32)
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) {" + Environment.NewLine;
                        out_str += "\t\t\t\t\tsize += " + sizeof(int) + ";" + Environment.NewLine;
                        out_str += "\t\t\t\t}" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_fixed64 || node.nodes[i].value_type == Token.eValueType.e_int64 || node.nodes[i].value_type == Token.eValueType.e_uint64)
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) {" + Environment.NewLine;
                        out_str += "\t\t\t\t\tsize += " + sizeof(long) + ";" + Environment.NewLine;
                        out_str += "\t\t\t\t}" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_float)
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) {" + Environment.NewLine;
                        out_str += "\t\t\t\t\tsize += " + sizeof(float) + ";" + Environment.NewLine;
                        out_str += "\t\t\t\t}" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_string)
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tbyte[] " + node.nodes[i].name + "_b;" + Environment.NewLine;

                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) {" + Environment.NewLine;

                        out_str += "\t\t\t\t\t" + node.nodes[i].name + "_b = System.Text.Encoding.UTF8.GetBytes(" + node.nodes[i].name + "[i]);" + Environment.NewLine;
                        out_str += "\t\t\t\t\tsize += GetStrActualSize(" + node.nodes[i].name + "_b.Length);" + Environment.NewLine;

                        
                        out_str += "\t\t\t\t}" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                }
            }

            if(sizeWeKonw>0)
                out_str += "\t\t\tsize += "+sizeWeKonw+";" + Environment.NewLine;
            out_str += "\t\t\treturn size;" + Environment.NewLine;
            out_str += "\t\t}" + Environment.NewLine;

            if (haveString)
            {
                out_str += "\t\tint GetStrActualSize(int size)\n\t\t{" + Environment.NewLine;
                out_str += "\t\t\tlong v = (long)size;" + Environment.NewLine;
                out_str += "\t\t\tv = (v << 1) ^ (v >> 31);" + Environment.NewLine;
                out_str += "\t\t\tif (v > 0)\n\t\t\t{" + Environment.NewLine;
                out_str += "\t\t\t\twhile (v > 0)\n\t\t\t\t{" + Environment.NewLine;
                out_str += "\t\t\t\tv = v >> 7;size++;\n\t\t\t\t}" + Environment.NewLine;
                out_str += "\t\t\t}\n\t\t\telse\n\t\t\t{" + Environment.NewLine;
                out_str += "\t\t\t\tsize++;" + Environment.NewLine;
                out_str += "\t\t\t}" + Environment.NewLine;
                out_str += "\t\t\treturn size;\n\t\t}" + Environment.NewLine;
            }

            return out_str;
        }

        #region Make ToString
        string DumpMakeString(MessageNode node)
        {
            string out_str = string.Empty;
            out_str += Environment.NewLine;
            out_str += "\t\tpublic string MakeString(){" + Environment.NewLine;
            out_str += "\t\t\tstring out_str = string.Empty;" + Environment.NewLine;
            for (int i = 0; i < node.nodes.Count; i++)
            {
                if (node.nodes[i].type == Token.eKeyWord.e_required || node.nodes[i].type == Token.eKeyWord.e_optional)
                {
                    if (node.nodes[i].value_type == Token.eValueType.e_byte)
                    {
                        out_str += MakeByteString(node.nodes[i]);
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_ref)
                    {
                        out_str += "\t\t\tout_str += \"\\n" + node.nodes[i].name + " = {\\n\";" + Environment.NewLine; 
                        out_str += "\t\t\tif (" + node.nodes[i].name+ " != null) out_str += " + node.nodes[i].name + ".MakeString();" + Environment.NewLine;
                        out_str += "\t\t\tout_str += \"}\\n\\n\";" + Environment.NewLine;
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
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) { out_str += \"" + node.nodes[i].name + "[\" + i + \"]\\n{\\n\" + string.Format(\"\\t{0}\", " + node.nodes[i].name + "[i].MakeString()) + \"}\\n\"; }" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                    else if (node.nodes[i].value_type == Token.eValueType.e_enum)
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) {out_str += \"" + node.nodes[i].name + "[\" + i + \"]\\n{\\n\\t\" + " + node.nodes[i].name +"[i] + \"\\n}\\n\";}" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                    else
                    {
                        out_str += "\t\t\tif (" + node.nodes[i].name + " != null) {" + Environment.NewLine;
                        out_str += "\t\t\t\tfor (int i = 0; i < " + node.nodes[i].name + ".Count; i++) {out_str += \"" + node.nodes[i].name + "[\" + i + \"]\\n{\\n\" + string.Format(\"\\t{0}\"," + node.nodes[i].name + "[i]) + \"\\n}\\n\";}" + Environment.NewLine;
                        out_str += "\t\t\t}" + Environment.NewLine;
                    }
                }
            }
            out_str += "\t\t\treturn out_str;" + Environment.NewLine;
            out_str += "\t\t}" + Environment.NewLine;
            return out_str;
        }

        string MakeByteString(ValueNode node)
        {
            string out_str = string.Empty;
            out_str += "\t\t\tout_str += \"" + node.name + " = \" + ProtocalDeSerialize.ByteToString(" + node.name + ");" + Environment.NewLine;
            return out_str;
        }

        string MakeCommonString(ValueNode node) {
            string out_str = string.Empty;
            out_str += "\t\t\tout_str += \"" + node.name + " = \" + string.Format(\"{0}\\n\"," + node.name + ");" + Environment.NewLine;
            return out_str;
        }

        #endregion
    }
}
