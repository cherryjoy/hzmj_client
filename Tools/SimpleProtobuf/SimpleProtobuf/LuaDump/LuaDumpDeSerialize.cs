using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtobuf
{
    
    class LuaDumpDeSerialize
    {
        public static LuaDumpDeSerialize create() { return new LuaDumpDeSerialize(); }
        public string MakeDeserialize(MessageNode msg_node)
        {
            string out_str = "function " + msg_node.name + ".DeSerialize(obj)" + Environment.NewLine;

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
                    inner_str += MakeDeSerializeOptional(val_node,ref count);
                }
                else if (val_node.type == Token.eKeyWord.e_repeated)
                {
                    inner_str += MakeDeSerializeRepeated(val_node,i);
                }
            }

            out_str += "\tlocal fieldCount = deserialize_int32()" + Environment.NewLine; 
            out_str += "\tlocal option,optionLength = deserialize_optional()" + Environment.NewLine;
            out_str += inner_str;
            out_str += "\treturn obj" + Environment.NewLine;
            out_str += "end" + Environment.NewLine+Environment.NewLine;
            return out_str;
        }

        private string MakeDeSerializeOptional(ValueNode node,ref int count)
        {
            string out_str = string.Empty;
            out_str += "\tif(has_optional_flag(" + count++ + ",optionLength,option)) then" + Environment.NewLine;
            out_str += MakeDeSerializeRequired(node, "\t\t");
            out_str += "\tend" + Environment.NewLine;
            return out_str;
        }

        private string MakeDeSerializeRequired(ValueNode node, string table = "\t")
        {
            string out_str = string.Empty;
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "obj." + node.name + " = deserialize_int32()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "obj." + node.name + " = deserialize_int64()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_longid)
            {
                out_str += table + "obj." + node.name + " = deserialize_long_id()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "obj." + node.name + " = deserialize_float()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "obj." + node.name + " = deserialize_string()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "obj." + node.name + " = deserialize_bool()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "obj." + node.name + ",obj." + node.name + "Length = deserialize_bytes()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "local " + node.name + "Size = deserialize_data_length()" + Environment.NewLine; 
                out_str += table + "local " + node.name + "StartPos = get_position()" + Environment.NewLine;
                out_str += table + "obj." + node.name + " = " + node.ref_name + ".Create()" + Environment.NewLine;
                out_str += table + "obj." + node.name + " = obj." + node.name + ":DeSerialize()" + Environment.NewLine;
                out_str += table + "set_position(" + node.name + "Size+" + node.name + "StartPos )" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "obj." + node.name + " = deserialize_int32()" + Environment.NewLine;
            }
            return out_str;
        }

        private string MakeDeSerializeRepeated(ValueNode node,int nodeIndex)
        {
            string out_str = "\n";
            string table = "\t\t";

            out_str += "\tif (fieldCount >= "+(nodeIndex+1)+") then" + Environment.NewLine;
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "obj." + node.name + " = deserialize_int32_array()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "obj." + node.name + " = deserialize_int64_array()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_longid)
            {
                out_str += table + "obj." + node.name + " = deserialize_long_id_array()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "obj." + node.name + " = deserialize_float_array()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "obj." + node.name + " = deserialize_string_array()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "obj." + node.name + " = deserialize_bool_array()" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                throw new Exception("错误：协议中包含了repeat byte类型的定义，请不要使用此类型的定义");
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "obj." + node.name + " = {}" + Environment.NewLine;
                out_str += table + "local len = deserialize_int32()" + Environment.NewLine;
                out_str += table + "if (len > 0) then" + Environment.NewLine;
                out_str += table + "\tfor i=1,len do" + Environment.NewLine;
                out_str += table + "\t\tlocal " + node.name + "Size = deserialize_data_length()" + Environment.NewLine;
                out_str += table + "\t\tlocal " + node.name + "StartPos = get_position()" + Environment.NewLine;
                out_str += table + "\t\tobj." + node.name + "[i] = " + node.ref_name + ".Create()" + Environment.NewLine;
                out_str += table + "\t\tobj." + node.name + "[i] = (obj." + node.name + "[i]):DeSerialize()" + Environment.NewLine;
                out_str += table + "\t\tset_position(" + node.name + "Size+" + node.name + "StartPos )" + Environment.NewLine;
                out_str += table + "\tend" + Environment.NewLine;
                out_str += table + "end" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "local len = deserialize_int32()" + Environment.NewLine;
                out_str += table + "if (len > 0) then" + Environment.NewLine;
                out_str += table + "\tobj." + node.name + " = {}" + Environment.NewLine;
                out_str += table + "\tfor i=1,len do" + Environment.NewLine;
                out_str += table + "\t\tobj." + node.name + "[i] = deserialize_int32()" + Environment.NewLine;
                out_str += table + "\tend" + Environment.NewLine;
                out_str += table + "end" + Environment.NewLine;
            }
            out_str += "\tend" + Environment.NewLine;
            return out_str;
        }
    }
}
