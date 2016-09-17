using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SimpleProtobuf
{
    class JavaDump
    {
        public bool isMakeString;
        public void DumpToFile(string pathname, ParseTree tree)
        {
           
            string import_class_name = string.Empty;
            for (int i = 0; i < tree.import_proto_.Count; i++)
            {
                string file_name = Path.GetFileNameWithoutExtension(tree.import_proto_[i].file_path_);

                tree.import_proto_[i].opions_.TryGetValue("java_outer_classname", out import_class_name);
                DumpToFile(pathname, tree.import_proto_[i]);
            }

            string java_file_name ;
            tree.opions_.TryGetValue("java_outer_classname", out java_file_name);

            string package_name;
            tree.opions_.TryGetValue("java_package", out package_name);
            string path = pathname +  package_name.Replace('.', '\\');
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            StreamWriter sw = new StreamWriter(Path.Combine(path, Path.GetFileNameWithoutExtension( java_file_name)) + ".java");
            sw.Write(Dump(tree, import_class_name));
            sw.Close();
        }

        public string Dump(ParseTree tree, string import = "")
        {
            string out_str = string.Empty;
            // dump namespace

            string package_name;
            tree.opions_.TryGetValue("java_package", out package_name);
            out_str += "package " + package_name + ";" + Environment.NewLine + Environment.NewLine;

            out_str += "import java.io.ByteArrayInputStream;\nimport java.io.ByteArrayOutputStream;\nimport java.io.InputStream;\nimport java.io.OutputStream;\nimport java.util.ArrayList;\nimport java.util.List;\nimport java.util.Collection;\n";
            out_str += "\nimport com.linekong.common.serialize.InvalidProtocolException;\nimport static com.linekong.common.serialize.ProtocalDeSerialize.*;\nimport static com.linekong.common.serialize.ProtocalSerialize.*;\nimport com.linekong.common.serialize.ProtocalInputStream;\n\n";

            if (string.IsNullOrEmpty(import) == false)
            {
                out_str += "import " + package_name + "." + import + ".*;" + Environment.NewLine + Environment.NewLine;
            }
            else
                out_str += "import " + package_name + ".*;" + Environment.NewLine + Environment.NewLine;
            
            string java_outer_classname;
            tree.opions_.TryGetValue("java_outer_classname", out java_outer_classname);
            out_str += "public final class " + java_outer_classname + " {" + Environment.NewLine;
            out_str += "final static int BUFFER_SIZE = 64;" + Environment.NewLine;

            foreach (EnumNode enum_node in tree.enums_.Values)
                out_str += DumpEnum(enum_node);
            for (int i = 0; i < tree.nodes_.Count; i++)
            {
                EnumNode enumNode;
                tree.enums_.TryGetValue(tree.nodes_[i].name, out enumNode);
                if (enumNode != null)
                {
                    enumNode.package_name = package_name;
                    enumNode.java_out_class = java_outer_classname;
                }
                out_str += DumpClass(tree.nodes_[i], enumNode, package_name, java_outer_classname, import);
            }

            out_str += "}" + Environment.NewLine;
            return out_str;
        }

        string DumpEnum(EnumNode enum_node)
        {
            string out_str = string.Empty;
            foreach (var v in enum_node.enums_)
            {
                out_str += "public enum " + v.Key  + Environment.NewLine;
                out_str += "{" + Environment.NewLine;
                Dictionary<string, object> items = v.Value;

                foreach (var i in items)
                {
                    out_str += "\t" + i.Key  + "," + Environment.NewLine;

                }
                out_str += "}" + Environment.NewLine + Environment.NewLine;
            }
            return out_str;
        
        }

        string DumpClass(MessageNode msg_node, EnumNode enum_node, string package_name, string java_outer_classname, string import_class)
        {
            string out_str = string.Empty;
            int count = 0;
            out_str += "public static final class " + msg_node.name  +"{"+ Environment.NewLine;

            out_str += "\tpublic static " + msg_node.name + " getInstance() {" + Environment.NewLine;
            out_str += "\t\treturn new " + msg_node.name + "();" + Environment.NewLine;
            out_str += "\t}" + Environment.NewLine;

            string temp = string.Empty;
            int msg_node_count = msg_node.nodes.Count;

            for (int i = 0; i < msg_node_count; i++)
            {
                temp += DumpValue(msg_node, msg_node.nodes[i], ref count);
            }

            int flagSize = (int)Math.Ceiling(count / 7.0f);
            flagSize = flagSize == 0 ? 1 : flagSize;
            
            out_str += "\tprivate byte[] flags_ = new byte[" + flagSize + "];" + Environment.NewLine;
            out_str += temp;
            out_str += Environment.NewLine;

            out_str += DumpDeSerialize(msg_node);

            out_str += DumpSerialize(msg_node, (int)Math.Ceiling(count / 7.0f));

            //if (isMakeString)
                //out_str += DumpMakeString(msg_node) + Environment.NewLine;

            out_str += "}" + Environment.NewLine + Environment.NewLine;
            return out_str;
        }

        #region 
        string DumpValue(MessageNode msg_node,ValueNode node, ref int count)
        {
            string out_str = string.Empty;
            if (node.type == Token.eKeyWord.e_repeated)
            {
                if (node.value_type == Token.eValueType.e_int32 ||
                    node.value_type == Token.eValueType.e_uint32 ||
                    node.value_type == Token.eValueType.e_fixed32)
                {
                    out_str += "\tpublic List<Integer> " + node.name + " = new ArrayList<Integer>();" + Environment.NewLine;
                    out_str += DumpListHelpFunc(msg_node, node, "Integer");
                }
                else if (node.value_type == Token.eValueType.e_float)
                {
                    out_str += "\tpublic List<Float> " + node.name + " = new ArrayList<Float>();" + Environment.NewLine;
                    out_str += DumpListHelpFunc(msg_node, node, "Float");
                }
                else if (node.value_type == Token.eValueType.e_int64 ||
                    node.value_type == Token.eValueType.e_uint64 ||
                    node.value_type == Token.eValueType.e_fixed64 ||
                    node.value_type == Token.eValueType.e_longid
                    )
                {
                    out_str += "\tpublic List<Long> " + node.name + " = new ArrayList<Long>();" + Environment.NewLine;
                    out_str += DumpListHelpFunc(msg_node, node, "Long");
                }
                else if (node.value_type == Token.eValueType.e_string)
                {
                    out_str += "\tpublic List<String> " + node.name + " = new ArrayList<String>();" + Environment.NewLine;
                    out_str += DumpListHelpFunc(msg_node, node, "String");
                }
                else if (node.value_type == Token.eValueType.e_bool)
                {
                    out_str += "\tpublic List<Boolean> " + node.name + " = new ArrayList<Boolean>();" + Environment.NewLine;
                    out_str += DumpListHelpFunc(msg_node, node, "Boolean");
                }
                else if (node.value_type == Token.eValueType.e_byte)
                {
                    throw new Exception("错误：协议中包含了repeat byte类型的定义，请不要使用此类型的定义");
                }
                else if (node.value_type == Token.eValueType.e_ref)
                {
                    out_str += "\tpublic List<" + node.ref_name + "> " + node.name + " = new ArrayList<" + node.ref_name + ">();" + Environment.NewLine;
                    out_str += DumpListHelpFunc(msg_node, node, node.ref_name);
                }
                else if (node.value_type == Token.eValueType.e_enum)
                {
                    out_str += "\tpublic List<" + node.ref_name + "> " + node.name + " = new ArrayList<" + node.ref_name + ">();" + Environment.NewLine;
                    out_str += DumpListHelpFunc(msg_node, node, node.ref_name);
                }
               
            }
            else if (node.type == Token.eKeyWord.e_optional)
            {
                if (node.value_type == Token.eValueType.e_int32 ||
                    node.value_type == Token.eValueType.e_uint32 ||
                    node.value_type == Token.eValueType.e_fixed32)
                {
                    out_str += "\tprivate int " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic int get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " + msg_node.name + " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(int " + node.name + "){" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_float)
                {
                    out_str += "\tprivate float " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic float get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(float "+node.name+"){";
                }
                else if (node.value_type == Token.eValueType.e_int64 ||
                    node.value_type == Token.eValueType.e_uint64 ||
                    node.value_type == Token.eValueType.e_fixed64 ||
                    node.value_type == Token.eValueType.e_longid)
                {
                    out_str += "\tprivate long " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic long get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(long "+node.name+"){";
                }
                else if (node.value_type == Token.eValueType.e_string)
                {
                    out_str += "\tprivate String " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic String get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(String "+node.name+"){";
                }
                else if (node.value_type == Token.eValueType.e_bool)
                {
                    out_str += "\tprivate boolean " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic boolean get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(boolean "+node.name+"){";
                }
                else if (node.value_type == Token.eValueType.e_byte)
                {
                    out_str += "\tprivate byte[] " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic byte[] get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return " + node.name + "_;}" + Environment.NewLine;
                    out_str += "\tpublic " + msg_node.name + " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(byte[] " + node.name + "){";
                }
                else if (node.value_type == Token.eValueType.e_ref)
                {
                    out_str += "\tprivate " + node.ref_name + " " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic " + node.ref_name + " get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return " + node.name + "_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "("+node.ref_name+" "+node.name+"){";
                }
                else if (node.value_type == Token.eValueType.e_enum)
                {
                    out_str += "\tprivate " + node.ref_name + " " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic " + node.ref_name + " get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return " + node.name + "_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "("+node.ref_name+" "+node.name+"){";
                }
                out_str += "\t\tthis." + node.name + "_ = " + node.name + "; " + Environment.NewLine;

                if (count % 7 == 0)
                    out_str += "\t\tbyte b = flags_[" + (count / 7) + "];  b |= 0x01; flags_[" + (count++ / 7) + "] = b;" + Environment.NewLine;
                else
                    out_str += "\t\tbyte b = flags_[" + (count / 7) + "];  b |= 0x01 << " + (count % 7) + "; flags_[" + (count++ / 7) + "] = b;" + Environment.NewLine;
                out_str += "\t\treturn this; " + Environment.NewLine;
                out_str += "\t}" + Environment.NewLine;

                //set aditional code for int optional
                if (node.value_type == Token.eValueType.e_int32 ||
                    node.value_type == Token.eValueType.e_uint32 ||
                    node.value_type == Token.eValueType.e_fixed32)
                {
                    out_str += "\tpublic " + msg_node.name + " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(int " + node.name + ", int defaultValue){" + Environment.NewLine;
                    out_str += "\t\tif(" + node.name + " != defaultValue){" + Environment.NewLine;
                    out_str += "\t\t\tset" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(" + node.name + ");" + Environment.NewLine;
                 
                    out_str += "\t\t}" + Environment.NewLine;
                    out_str += "\t\treturn this; " + Environment.NewLine;
                    out_str += "\t}" + Environment.NewLine;
                }
            }
            else
            {
                if (node.value_type == Token.eValueType.e_int32 ||
                    node.value_type == Token.eValueType.e_uint32 ||
                    node.value_type == Token.eValueType.e_fixed32)
                {
                    out_str += "\tprivate int " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic int get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(int "+node.name+"){this." + node.name + "_ = "+node.name+";return this;}" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_float)
                {
                    out_str += "\tprivate float " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic float get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(float "+node.name+"){this." + node.name + "_ = "+node.name+";return this;}" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_int64 ||
                    node.value_type == Token.eValueType.e_uint64 ||
                    node.value_type == Token.eValueType.e_fixed64 ||
                    node.value_type == Token.eValueType.e_longid)
                {
                    out_str += "\tprivate long " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic long get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(long "+node.name+"){this." + node.name + "_ = "+node.name+";return this;}" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_string)
                {
                    out_str += "\tprivate String " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic String get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(String "+node.name+"){this." + node.name + "_ = "+node.name+";return this;}" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_bool)
                {
                    out_str += "\tprivate boolean " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic boolean get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return "+node.name+"_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(boolean "+node.name+"){this." + node.name + "_ = "+node.name+";return this;}" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_byte)
                {
                    out_str += "\tprivate byte[] " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic byte[] get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return " + node.name + "_;}" + Environment.NewLine;
                    out_str += "\tpublic " + msg_node.name + " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(byte[] " + node.name + "){this." + node.name + "_ = " + node.name + ";return this;}" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_ref)
                {
                    out_str += "\tprivate " + node.ref_name + " " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic " + node.ref_name + " get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return " + node.name + "_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(" + node.ref_name + " " + node.name + "){this." + node.name + "_ = " + node.name + ";return this;}" + Environment.NewLine;
                }
                else if (node.value_type == Token.eValueType.e_enum)
                {
                    out_str += "\tprivate " + node.ref_name + " " + node.name + "_;" + Environment.NewLine;
                    out_str += "\tpublic " + node.ref_name + " get" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(){return " + node.name + "_;}" + Environment.NewLine;
                    out_str += "\tpublic " +msg_node.name+ " set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(" + node.ref_name + " " + node.name + "){this." + node.name + "_ = " + node.name + ";return this;}" + Environment.NewLine;
                }
            }

            return out_str;
        }

        string DumpListHelpFunc(MessageNode msg_node,ValueNode node,string refName) {
            string out_str = "";
            out_str += "\tpublic " + msg_node.name + " add" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(" + refName + " value){" + Environment.NewLine;
            out_str += "\t\t" + node.name + ".add(value);" + Environment.NewLine;
            out_str += "\t\treturn this;" + Environment.NewLine;
            out_str += "\t}" + Environment.NewLine;

            out_str += "\tpublic " + msg_node.name + " addAll" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(Collection<? extends " + refName + "> values){" + Environment.NewLine;
            out_str += "\t\t" + node.name + ".addAll(values);" + Environment.NewLine;
            out_str += "\t\treturn this;" + Environment.NewLine;
            out_str += "\t}" + Environment.NewLine;
            return out_str;
        }
        #endregion

        #region DeSerialize
        string DumpDeSerialize(MessageNode msg_node) {
            string out_str = "\tpublic static " + msg_node.name + " deSerialize(byte[] buffer) throws InvalidProtocolException {" + Environment.NewLine;
            out_str += "\t\treturn deSerialize(new ProtocalInputStream(buffer));" + Environment.NewLine;
            out_str += "\t}" + Environment.NewLine;

            out_str += "\tpublic static " + msg_node.name + " deSerialize(ProtocalInputStream is) throws InvalidProtocolException {" + Environment.NewLine;
            out_str += "\t\t" + msg_node.name + " obj = new " + msg_node.name + "();" + Environment.NewLine;
            out_str += "\t\tint fieldCount = deSerializeInt32(is);" + Environment.NewLine;
 

            int count = 0;
            string inner_str = string.Empty;
            for (int i = 0; i < msg_node.nodes.Count; i++)
            {
                ValueNode val_node = msg_node.nodes[i];
                if (val_node.type == Token.eKeyWord.e_required)
                {
                    inner_str += MakeDeSerializeRequired(val_node);
                }
                else if (val_node.type == Token.eKeyWord.e_optional)
                {
                    inner_str += MakeDeSerializeOptional(val_node, ref count);
                }
                else if (val_node.type == Token.eKeyWord.e_repeated)
                {
                    inner_str += MakeDeSerializeRepeated(val_node,i);
                }
            }

            out_str += "\t\tByte[] options = deSerializeOptional(is);" + Environment.NewLine;
            out_str += inner_str;
            out_str += "\t\treturn obj;" + Environment.NewLine;
            out_str += "\t}" + Environment.NewLine;

            out_str += "\tpublic static " + msg_node.name + " deSerializeReference(ProtocalInputStream is) throws InvalidProtocolException {" + Environment.NewLine;
            out_str += "\t\tint dataLen = deserializeInt32Normal(is);" + Environment.NewLine;
            out_str += "\t\tint pos = is.getPos();" + Environment.NewLine;
            out_str += "\t\t"+ msg_node.name+" result = deSerialize(is);" + Environment.NewLine;
            out_str += "\t\tint readCount = is.getPos() - pos;" + Environment.NewLine;
            out_str += "\t\tis.skip(readCount, dataLen);" + Environment.NewLine;
            out_str += "\t\treturn result;" + Environment.NewLine;

            out_str += "\t}" + Environment.NewLine;

            return out_str;

        }

        private string MakeDeSerializeOptional(ValueNode node, ref int count)
        {
            string out_str = string.Empty;
            out_str += "\t\tif (hasOptionalFlag(options, " + count++ + "))";
            out_str += MakeDeSerializeRequired(node, "");
            return out_str;
        }

        private string MakeDeSerializeRequired(ValueNode node, string table = "\t\t")
        {
            string out_str = string.Empty;
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "obj.set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(deSerializeInt32(is));" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64 ||
                    node.value_type == Token.eValueType.e_longid)
            {
                out_str += table + "obj.set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(deSerializeInt64(is));" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "obj.set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(deSerializeFloat(is));" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "obj.set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(deSerializeString(is));" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "obj.set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(deSerializeBool(is));" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "obj.set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(deSerializeBytes(is));" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "obj.set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(" + node.ref_name + ".deSerializeReference(is));" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "obj.set" + char.ToUpper(node.name[0]) + node.name.Substring(1) + "(" + node.ref_name + ".values()[deSerializeInt32(is)]);" + Environment.NewLine;
            }
            return out_str;
        }

        private string MakeDeSerializeRepeated(ValueNode node,int nodeIndex)
        {
            string out_str = string.Empty;
            out_str += "\t\t" + "if(fieldCount >=" + (nodeIndex + 1) + "){"+Environment.NewLine;
            string table = "\t\t\t";
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "obj." + node.name + " = deSerializeIntList(is);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64 ||
                    node.value_type == Token.eValueType.e_longid)
            {
                out_str += table + "obj." + node.name + " = deSerializeLongList(is);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "obj." + node.name + " = deSerializeFloatList(is);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "obj." + node.name + " = deSerializeStringList(is);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "obj." + node.name + " = deSerializeBoolList(is);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                 throw new Exception("错误：协议中包含了repeat byte类型的定义，请不要使用此类型的定义");
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "{" + Environment.NewLine;
                out_str += table + "\t" + "int len = deSerializeLength(is);" + Environment.NewLine;
                out_str += table + "\tif (len > 0){" + Environment.NewLine;
                out_str += table + "\tobj."+node.name+" = new ArrayList<"+node.ref_name+">(len);" + Environment.NewLine;
                out_str += table + "\tfor (int i = 0; i < len; i++) { obj." + node.name + ".add(" + node.ref_name + ".deSerializeReference(is));}" + Environment.NewLine;
                out_str += table + "\t} else {" + Environment.NewLine;
                out_str += table + "\t\tobj."+node.name+" = new ArrayList<"+node.ref_name+">();" + Environment.NewLine;
                out_str += table + "\t}" + Environment.NewLine;
                out_str += table + "}" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "{" + Environment.NewLine;
                out_str += table + "\tint len = deSerializeLength(is);" + Environment.NewLine;
                out_str += table + "\tif (len > 0){" + Environment.NewLine;
                out_str += table + "\tobj."+node.name+" = new ArrayList<"+node.ref_name+">(len);" + Environment.NewLine;
                out_str += table + "\tfor (int i = 0; i < len; i++) { obj." + node.name + ".add(" + node.ref_name + ".values()[deSerializeInt32(is)]); }" + Environment.NewLine;
                out_str += table + "\t} else {" + Environment.NewLine;
                out_str += table + "\t\tobj." + node.name + " = new ArrayList<" + node.ref_name + ">();" + Environment.NewLine;
                out_str += table + "\t}" + Environment.NewLine;
                out_str += table + "}" + Environment.NewLine;
            }
            out_str += "\t\t}"+Environment.NewLine;
            return out_str;
        }
        #endregion

        #region Serialize
        string DumpSerialize(MessageNode msg_node, int count)
        {
            string out_str = string.Empty;

            out_str += "\tpublic byte[] serialize() {" + Environment.NewLine;
            out_str += "\t\tByteArrayOutputStream bos = new ByteArrayOutputStream(BUFFER_SIZE);\n\t\tserialize(bos);\n\t\treturn bos.toByteArray();\n\t}" + Environment.NewLine;


            out_str += "\tpublic void serialize(OutputStream os){" + Environment.NewLine;
            int optionCount = 0;
            out_str += "\t\tint fieldCount = " + msg_node.nodes.Count + ";" + Environment.NewLine;
            out_str += "\t\tserializeInt32(os,fieldCount);" + Environment.NewLine;

            out_str += "\t\tserializeOptional(os, flags_);" + Environment.NewLine;
            for (int i = 0; i < msg_node.nodes.Count; i++)
            {
                ValueNode node = msg_node.nodes[i];
                if (node.type == Token.eKeyWord.e_required)
                {
                    out_str += MakeSerializeRequired(node);
                }
                else if (node.type == Token.eKeyWord.e_optional)
                {
                    out_str += MakeSerializeOptional(node, ref optionCount);
                }
                else if (node.type == Token.eKeyWord.e_repeated)
                {
                    out_str += MakeSerializeRepeated(node);
                }
            }
            out_str += "\t}" + Environment.NewLine;


            out_str += "\tpublic void serializeReference(OutputStream os) {" + Environment.NewLine;
            out_str += "\t\tByteArrayOutputStream baos = new ByteArrayOutputStream(BUFFER_SIZE / 2);" + Environment.NewLine;
            out_str += "\t\tserialize(baos);" + Environment.NewLine;
            out_str += "\t\tserializeReferenceBytes(os, baos.toByteArray());" + Environment.NewLine;
            out_str += "\t}" + Environment.NewLine;

            return out_str;
        
        }

        private string MakeSerializeRequired(ValueNode node)
        {
            string out_str = string.Empty;
            string table = "\t\t";
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "serializeInt32(os, " + node.name + "_);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64 ||
                    node.value_type == Token.eValueType.e_longid)
            {
                out_str += table + "serializeInt64(os, " + node.name + "_);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "serializeFloat(os, " + node.name + "_);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "serializeString(os, " + node.name + "_);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "serializeBool(os, " + node.name + "_);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "serializeBytes(os, " + node.name + "_);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "if (" + node.name + "_ != null) " + node.name + "_.serializeReference(os);" + Environment.NewLine;
                out_str += table + "else throw new IllegalArgumentException(\"missing required type "+node.name+"\");"+ Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "serializeInt32(os, " + node.name + "_.ordinal());" + Environment.NewLine;
            }
            return out_str;
        }

        private string MakeSerializeOptional(ValueNode val_node, ref int idx)
        {
            string out_str = string.Empty;
            string table = "\t\t";
            out_str += table + "if(hasOptionalFlag(flags_, " + idx++ + ")){" + Environment.NewLine;
            out_str += "\t" + MakeSerializeRequired(val_node);
            out_str += table + "}" + Environment.NewLine;
            return out_str;
        }

        private string MakeSerializeRepeated(ValueNode node)
        {
            string out_str = string.Empty;
            string table = "\t\t";
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "serializeInt32List(os, " + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64 ||
                    node.value_type == Token.eValueType.e_longid)
            {
                out_str += table + "serializeInt64List(os," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "serializeFloatList(os," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "serializeStringList(os," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "serializeBoolList(os," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                throw new Exception("错误：协议中包含了repeat byte类型的定义，请不要使用此类型的定义");
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "if (" + node.name + " != null) {" + Environment.NewLine;
                out_str += table + "\tint len = " + node.name + ".size();  serializeInt32(os, len);" + Environment.NewLine;
                out_str += table + "\tfor (int i = 0; i < len; i++) { " + node.name + ".get(i).serializeReference(os);}" + Environment.NewLine;
                out_str += table + "} else serializeInt32(os, 0);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "if (" + node.name + " != null) {" + Environment.NewLine;
                out_str += table + "\tint len = " + node.name + ".size(); List<Integer> list = new ArrayList<Integer>(len); for (int i = 0; i < len; i++) { list.add(" + node.name + ".get(i).ordinal()); }" + Environment.NewLine;
                out_str += table + "\tserializeInt32List(os ,list);" + Environment.NewLine;
                out_str += table + "} else serializeInt32(os, 0);" + Environment.NewLine;
            }
            return out_str;
        }
        #endregion

        #region Make ToString
       

        #endregion
    }
}
