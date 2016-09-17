using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtobuf
{
    class LuaDumpSerialize
    {
        public static LuaDumpSerialize create()
        {
            return new LuaDumpSerialize();
        }

        public string MakeSerialize(MessageNode msg_node, int count)
        {
            string out_str = "";
            out_str += "function " + msg_node.name + ".Serialize(obj)" + Environment.NewLine;
            out_str += "\tlocal fieldCount = " + msg_node.nodes.Count + Environment.NewLine; 
            out_str += "\tserialize_int32(fieldCount)" + Environment.NewLine; 
            out_str += "\tlocal flag = {}" + Environment.NewLine;
            int index = 0;
            foreach (ValueNode node in msg_node.nodes)
            {
                if (node.type == Token.eKeyWord.e_optional)
                {
                    index++;
                    out_str += "\tif(obj." + node.name + " ~= nil) then" + Environment.NewLine;
                    out_str += "\t\tflag[" + index + "] = true" + Environment.NewLine;
                    out_str += "\telse" + Environment.NewLine;
                    out_str += "\t\tflag[" + index + "] = false" + Environment.NewLine;
                    out_str += "\tend" + Environment.NewLine;
                }
            }
            out_str += "\tserialize_optional(flag)" + Environment.NewLine;
            
            int optionCount = 0;
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

            out_str += "end" + Environment.NewLine;

            return out_str;
        }

        private string MakeSerializeRequired(ValueNode node,bool isFromOptional = false)
        {
            string out_str = string.Empty;
            string table = "\t";
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "serialize_int32(obj." + node.name + ")" + Environment.NewLine; 
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "serialize_int64(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_longid)
            {
                out_str += table + "serialize_long_id(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "serialize_float(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "serialize_string(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "serialize_bool(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                out_str += table + "serialize_bytes(obj." + node.name + ",obj."+node.name+"Length)" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                if (isFromOptional)
                {
                    out_str += table + "local WaitPos = get_position()" + Environment.NewLine;
                    out_str += table + "set_position(WaitPos+4)" + Environment.NewLine;
                    out_str += table + "obj." + node.name + ":Serialize()" + Environment.NewLine;
                    out_str += table + "local MaxPosNow = get_position()" + Environment.NewLine;
                    out_str += table + "local RealSize = MaxPosNow-(WaitPos+4)" + Environment.NewLine;
                    out_str += table + "set_position(WaitPos)" + Environment.NewLine;
                    out_str += table + "serialize_data_length(RealSize)" + Environment.NewLine;
                    out_str += table + "set_position(MaxPosNow)" + Environment.NewLine;
                   
                }
                else
                {
                    out_str += table + "if (obj." + node.name + " ~= nil) then" + Environment.NewLine;
                    out_str += table + table + "local WaitPos = get_position()" + Environment.NewLine;
                    out_str += table + table + "set_position(WaitPos+4)" + Environment.NewLine;
                    out_str += table + table + "obj." + node.name + ":Serialize()" + Environment.NewLine;
                    out_str += table + table + "local MaxPosNow = get_position()" + Environment.NewLine;
                    out_str += table + table + "local RealSize = MaxPosNow-(WaitPos+4)" + Environment.NewLine;
                    out_str += table + table + "set_position(WaitPos)" + Environment.NewLine;
                    out_str += table + table + "serialize_data_length(RealSize)" + Environment.NewLine;
                    out_str += table + table + "set_position(MaxPosNow)" + Environment.NewLine;
                    out_str += table + "else" + Environment.NewLine;
                    out_str += table + table + "LuaDebug.Log(\"require类型的参数必须赋值！" + node.name + "没有赋值！\" )" + Environment.NewLine;
                    out_str += table + "end" + Environment.NewLine;
                }
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "serialize_int32(obj." + node.name + ")" + Environment.NewLine;
            }
            return out_str;
        }

        private string MakeSerializeOptional(ValueNode val_node, ref int idx)
        {
            string out_str = string.Empty;
            string table = "\t";
            out_str += table + "if(obj." + val_node.name + " ~= nil) then" + Environment.NewLine;
            out_str += "\t" + MakeSerializeRequired(val_node,true);
            out_str += table + "end" + Environment.NewLine;
            return out_str;
        }

        private string MakeSerializeRepeated(ValueNode node)
        {
            string out_str = string.Empty;
            string table = "\t";
            if (node.value_type == Token.eValueType.e_int32 ||
                node.value_type == Token.eValueType.e_uint32 ||
                node.value_type == Token.eValueType.e_fixed32)
            {
                out_str += table + "serialize_int32_array(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_int64 ||
                node.value_type == Token.eValueType.e_uint64 ||
                node.value_type == Token.eValueType.e_fixed64)
            {
                out_str += table + "serialize_int64_array(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_longid)
            {
                out_str += table + "serialize_long_id_array(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_float)
            {
                out_str += table + "serialize_float_array(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_string)
            {
                out_str += table + "serialize_string_array(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_bool)
            {
                out_str += table + "serialize_bool_array(obj." + node.name + ")" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_byte)
            {
                throw new Exception("错误：协议中包含了repeat byte类型的定义，请不要使用此类型的定义");
            }
            else if (node.value_type == Token.eValueType.e_ref)
            {
                out_str += table + "if (obj." + node.name + " ~= nil) then" + Environment.NewLine;
                out_str += table + "\tserialize_int32(#(obj."+node.name+"))" + Environment.NewLine;
                out_str += table + "\tfor i=1, #(obj." + node.name + ") do" + Environment.NewLine;
                out_str += table + "\t\tlocal WaitPos = get_position()" + Environment.NewLine;
                out_str += table + "\t\tset_position(WaitPos+4)" + Environment.NewLine;
                out_str += table + "\t\tobj." + node.name + "[i]:Serialize()" + Environment.NewLine;
                out_str += table + "\t\tlocal MaxPosNow = get_position()" + Environment.NewLine;
                out_str += table + "\t\tlocal RealSize = MaxPosNow-(WaitPos+4)" + Environment.NewLine;
                out_str += table + "\t\tset_position(WaitPos)" + Environment.NewLine;
                out_str += table + "\t\tserialize_data_length(RealSize)" + Environment.NewLine;
                out_str += table + "\t\tset_position(MaxPosNow)" + Environment.NewLine;
                out_str += table + "\tend" + Environment.NewLine;
                out_str += table + "else" + Environment.NewLine;
                out_str += table + "\tserialize_int32(0)" + Environment.NewLine;
  
                out_str += table + "end" + Environment.NewLine;
            }
            else if (node.value_type == Token.eValueType.e_enum)
            {
                out_str += table + "if (obj." + node.name + " ~= nil) then" + Environment.NewLine;
                out_str += table + "\tlocal len = #(obj." + node.name + ")" + Environment.NewLine;
                out_str += table + "\tlocal enumTable = {}" + Environment.NewLine;
                out_str += table + "\tfor i=1, len do" + Environment.NewLine;
                out_str += table + "\t\tenumTable[i] = obj."+node.name+"[i]"+ Environment.NewLine;
                out_str += table + "\tend" + Environment.NewLine;

                out_str += table + "\tserialize_int32_array(enumTable)" + Environment.NewLine;
                out_str += table + "else" + Environment.NewLine;
                out_str += table + "\tserialize_int32(0)" + Environment.NewLine;
                out_str += table + "end" + Environment.NewLine;
            }
            return out_str;
        }

       
    }
}
