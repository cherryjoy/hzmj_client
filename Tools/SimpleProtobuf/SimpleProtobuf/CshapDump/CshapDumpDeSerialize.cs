using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtobuf
{
    class CshapDumpDeSerialize
    {
        public static CshapDumpDeSerialize create() { return new CshapDumpDeSerialize(); }
        public string MakeDeserialize(MessageNode msg_node)
        {
            string out_str = "\t\tpublic static " + msg_node.name + " DeSerialize(byte[] buffer,ref int _posIndex){" + Environment.NewLine;
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
                out_str += "\t\t\tbyte[] options = ProtocalDeSerialize.DeSerializeOptional(buffer,ref _posIndex);" + Environment.NewLine;
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
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeInt32(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeInt64(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeFloat(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeString(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeBool(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeBytes(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "obj." + node.name + " = " + node.ref_name + ".DeSerialize(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "obj." + node.name + " = (" + node.ref_name + ")ProtocalDeSerialize.DeSerializeInt32(buffer,ref _posIndex);" + Environment.NewLine;
            }
            return out_str;
        }

        private string MakeDeSerializeRepeated(ValueNode node)
        {
            string out_str = string.Empty;
            string table = "\t\t\t";
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeIntArray(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeLongArray(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeserializeFloatArray(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeStringArray(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeBoolArray(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "obj." + node.name + " = ProtocalDeSerialize.DeSerializeByteArray(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "{" + Environment.NewLine;
                out_str += table + "\t" + "int len = ProtocalDeSerialize.DeSerializeInt32(buffer,ref _posIndex);" + Environment.NewLine;
                out_str += table + "\tif (len > 0){" + Environment.NewLine;
                out_str += table + "\t\tfor (int i = 0; i < len; i++) { obj." + node.name + ".Add(" + node.ref_name + ".DeSerialize(buffer,ref _posIndex));}" + Environment.NewLine;
                out_str += table + "\t}" + Environment.NewLine;
                out_str += table + "}" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "{" + Environment.NewLine;
                out_str += table + "\tint len = ProtocalDeSerialize.DeSerializeInt32(buffer,ref _posIndex);" + Environment.NewLine;
                out_str += table + "\tif (len > 0){" + Environment.NewLine;
                out_str += table + "\t\tfor (int i = 0; i < len; i++) { obj." + node.name + ".Add((" + node.ref_name + ")ProtocalDeSerialize.DeSerializeInt32(buffer,ref _posIndex)); }" + Environment.NewLine;
                out_str += table + "\t}" + Environment.NewLine;
                out_str += table + "}" + Environment.NewLine;
            }
            return out_str;
        }
    }
}
