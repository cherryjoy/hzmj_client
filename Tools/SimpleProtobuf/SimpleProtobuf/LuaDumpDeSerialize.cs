using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtobuf
{
    class LuaDumpDeSerialize
    {
        public static CshapDumpDeSerialize create() { return new CshapDumpDeSerialize(); }
        public string MakeDeserialize(MessageNode msg_node)
        {
            string out_str = "\t\tpublic static " + msg_node.name + " DeSerialize(byte[] buffer)" + Environment.NewLine;
            out_str += "\t\t{" + Environment.NewLine;
            out_str += "\t\t\tSimpleProtobuf.ProtocalDeSerialize pdUtil = SimpleProtobuf.ProtocalDeSerialize.create(buffer);" + Environment.NewLine;
            out_str += "\t\t\treturn DeSerialize(pdUtil);" + Environment.NewLine;
            out_str += "\t\t}" + Environment.NewLine + Environment.NewLine;

            out_str += "\t\tpublic static " + msg_node.name + " DeSerialize(SimpleProtobuf.ProtocalDeSerialize pdUtil){" + Environment.NewLine;
            out_str += "\t\t\t" + msg_node.name + " obj = new " + msg_node.name + "();" + Environment.NewLine;

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
                    inner_str += MakeDeSerializeRepeated(val_node);
                }
            }
            if (count > 0)
            {
                out_str += "\t\t\tbyte[] options = pdUtil.DeSerializeOptional();" + Environment.NewLine;
            }
            out_str += inner_str;
            out_str += "\t\t\treturn obj;" + Environment.NewLine;
            out_str += "\t\t}" + Environment.NewLine;
            return out_str;
        }

        private string MakeDeSerializeOptional(ValueNode node, ref int count)
        {
            string out_str = string.Empty;
            out_str += "\t\t\tif (ProtocalSerialize.HasOptionalFlag(options, " + count++ + "))";
            out_str += MakeDeSerializeRequired(node, "");
            return out_str;
        }

        private string MakeDeSerializeRequired(ValueNode node, string table = "\t\t\t")
        {
            string out_str = string.Empty;
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeInt32();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeInt64();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeFloat();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeString();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeBool();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeBytes();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "obj." + node.name + " = " + node.ref_name + ".DeSerialize(pdUtil);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "obj." + node.name + " = (" + node.ref_name + ")pdUtil.DeSerializeInt32();" + Environment.NewLine;
            }
            return out_str;
        }

        private string MakeDeSerializeRepeated(ValueNode node)
        {
            string out_str = string.Empty;
            string table = "\t\t\t";
            if (node.value_type == Token.eValueType.e_int32||                
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeIntArray();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeLongArray();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeserializeFloatArray();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeStringArray();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeBoolArray();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "obj." + node.name + " = pdUtil.DeSerializeByteArray();" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "{" + Environment.NewLine;
                out_str += table + "\t" + "int len = pdUtil.DeSerializeInt32();" + Environment.NewLine;
                out_str += table + "\tif (len > 0){" + Environment.NewLine;
                out_str += table + "\t\tfor (int i = 0; i < len; i++) { obj." + node.name + ".Add(" + node.ref_name + ".DeSerialize(pdUtil));}" + Environment.NewLine;
                out_str += table + "\t}" + Environment.NewLine;
                out_str += table + "}" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "{" + Environment.NewLine;
                out_str += table + "\tint len = pdUtil.DeSerializeInt32();" + Environment.NewLine;
                out_str += table + "\tif (len > 0){" + Environment.NewLine;
                out_str += table + "\t\tfor (int i = 0; i < len; i++) { obj." + node.name + ".Add((" + node.ref_name + ")pdUtil.DeSerializeInt32()); }" + Environment.NewLine;
                out_str += table + "\t}" + Environment.NewLine;
                out_str += table + "}" + Environment.NewLine;
            }
            return out_str;
        }
    }
}
