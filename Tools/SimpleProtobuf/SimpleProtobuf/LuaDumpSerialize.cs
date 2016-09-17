using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtobuf
{
    class LuaDumpSerialize
    {
        public static CshapDumpSerialize create()
        {
            return new CshapDumpSerialize();
        }

        public string MakeSerialize(MessageNode msg_node, int count)
        {
            string out_str = string.Empty;
            out_str += "\t\tprivate byte[] buffer;" + Environment.NewLine;
            out_str += "\t\tpublic void Serialize()" + Environment.NewLine;
            out_str += "\t\t{" + Environment.NewLine;
            out_str += "\t\t\tSimpleProtobuf.ProtocalSerialize pUtil = SimpleProtobuf.ProtocalSerialize.create();" + Environment.NewLine;
            out_str += "\t\t\tSerialize(pUtil);" + Environment.NewLine;
            out_str += "\t\t}" + Environment.NewLine + Environment.NewLine;

            out_str += "\t\tpublic void Serialize(SimpleProtobuf.ProtocalSerialize pUtil)" + Environment.NewLine;
            out_str += "\t\t{" + Environment.NewLine;
            int optionCount = 0;
            if (count > 0)
                out_str += "\t\t\tpUtil.serializeOptional(flags_);" + Environment.NewLine;
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
            out_str += "\t\t\tbuffer = pUtil.toArray();" + Environment.NewLine;
            out_str += "\t\t}" + Environment.NewLine;

            out_str += Environment.NewLine;
            out_str += "\t\tpublic byte[] toArray(){" + Environment.NewLine;
            out_str += "\t\t\treturn buffer;" + Environment.NewLine;
            out_str += "\t\t}" + Environment.NewLine;
            return out_str;
        }

        private string MakeSerializeRequired(ValueNode node)
        {
            string out_str = string.Empty;
            string table = "\t\t\t";
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "pUtil.serializeInt32(" + node.name + ");" + Environment.NewLine; 
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "pUtil.serializeInt64(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "pUtil.serializeFloat(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "pUtil.serializeString(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "pUtil.serializeBool(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "pUtil.serializeByte(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "if (" + node.name + " != null) " + node.name + ".Serialize(pUtil);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "pUtil.serializeInt32((int)" + node.name + ");" + Environment.NewLine;
            }
            return out_str;
        }

        private string MakeSerializeOptional(ValueNode val_node, ref int idx)
        {
            string out_str = string.Empty;
            string table = "\t\t\t";
            out_str += table + "if(ProtocalSerialize.HasOptionalFlag(flags_, " + idx++ +")){" + Environment.NewLine;
            out_str += "\t" + MakeSerializeRequired(val_node);
            out_str += table + "}" + Environment.NewLine;
            return out_str;
        }

        private string MakeSerializeRepeated(ValueNode node)
        {
            string out_str = string.Empty;
            string table = "\t\t\t";
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "pUtil.serializeInt32Array(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "pUtil.serializeInt64Array(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "pUtil.serializeFloatArray(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "pUtil.serializeStringArray(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "pUtil.serializeBoolArray(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "pUtil.serializeBytes(" + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "if (" + node.name + " != null) {" + Environment.NewLine;
                out_str += table + "\tint len = " + node.name + ".Count; pUtil.serializeInt32(len);" + Environment.NewLine;
                out_str += table + "\tfor (int i = 0; i < len; i++) { " + node.name + "[i].Serialize(pUtil);}" + Environment.NewLine;
                out_str += table + "} else pUtil.serializeInt32(0);" + Environment.NewLine;           
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "if (" + node.name + " != null) {" + Environment.NewLine;
                out_str += table + "\tint len = " + node.name + ".Count; List<int> list = new List<int>(); for (int i = 0; i < len; i++) { list.Add((int)" + node.name + "[i]); }" + Environment.NewLine;
                out_str += table + "\tpUtil.serializeInt32Array(list);" + Environment.NewLine;
                out_str += table + "} else pUtil.serializeInt32(0);" + Environment.NewLine;
            }
            return out_str;
        }

       
    }
}
