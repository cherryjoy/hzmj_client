using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtobuf
{
    class CshapDumpSerialize
    {
        public static CshapDumpSerialize create()
        {
            return new CshapDumpSerialize();
        }

        public string MakeSerialize(MessageNode msg_node, int count)
        {
            string out_str = string.Empty;

            out_str += "\t\tpublic void Serialize(byte[] buffer,ref int _posIndex)" + Environment.NewLine;
            out_str += "\t\t{" + Environment.NewLine;
            int optionCount = 0;
            if (count > 0)
                out_str += "\t\t\tProtocalSerialize.serializeOptional(buffer,ref _posIndex,flags_);" + Environment.NewLine;
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
                out_str += table + "ProtocalSerialize.serializeInt32(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine; 
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "ProtocalSerialize.serializeInt64(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "ProtocalSerialize.serializeFloat(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "ProtocalSerialize.serializeString(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "ProtocalSerialize.serializeBool(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "ProtocalSerialize.serializeByte(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "if (" + node.name + " != null) " + node.name + ".Serialize(buffer,ref _posIndex);" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "ProtocalSerialize.serializeInt32(buffer,ref _posIndex,(int)" + node.name + ");" + Environment.NewLine;
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
                out_str += table + "ProtocalSerialize.serializeInt32Array(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "ProtocalSerialize.serializeInt64Array(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "ProtocalSerialize.serializeFloatArray(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "ProtocalSerialize.serializeStringArray(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "ProtocalSerialize.serializeBoolArray(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "ProtocalSerialize.serializeBytes(buffer,ref _posIndex," + node.name + ");" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "if (" + node.name + " != null) {" + Environment.NewLine;
                out_str += table + "\tint len = " + node.name + ".Count; ProtocalSerialize.serializeInt32(buffer,ref _posIndex,len);" + Environment.NewLine;
                out_str += table + "\tfor (int i = 0; i < len; i++) { " + node.name + "[i].Serialize(buffer,ref _posIndex);}" + Environment.NewLine;
                out_str += table + "} else ProtocalSerialize.serializeInt32(buffer,ref _posIndex,0);" + Environment.NewLine;           
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "if (" + node.name + " != null) {" + Environment.NewLine;
                out_str += table + "\tint len = " + node.name + ".Count; List<int> list = new List<int>(); for (int i = 0; i < len; i++) { list.Add((int)" + node.name + "[i]); }" + Environment.NewLine;
                out_str += table + "\tProtocalSerialize.serializeInt32Array(buffer,ref _posIndex,list);" + Environment.NewLine;
                out_str += table + "} else ProtocalSerialize.serializeInt32(buffer,ref _posIndex,0);" + Environment.NewLine;
            }
            return out_str;
        }

       
    }
}
