using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SimpleProtobuf
{
    class ValueNode
    {
        public string name;
        public int index;
        public object defult_value;
        public string ref_name;
        public Token.eKeyWord type;
        public Token.eValueType value_type;
        public string ToString()
        {
            return "name: " + name + "  index: " + index + " defult_value: " + defult_value + " ref_name: " + ref_name + " type: " + type + " value_type: " + value_type + "\n";
        }
    }

    class MessageNode
    {
        public string name;
        public List<ValueNode> nodes = new List<ValueNode>();

    }

    class EnumNode
    {
        public string message_name;
        public string package_name;
        public string java_out_class;
        public string enum_name;
        public Dictionary<string, Dictionary<string, object>> enums_ = new Dictionary<string, Dictionary<string, object>>();
    }

    class ParseTree
    {
        public string package_name_;
        public string file_path_;
        public List<ParseTree> import_proto_ = new List<ParseTree>();
        public Dictionary<string, string> opions_ = new Dictionary<string, string>();
        public Dictionary<string, EnumNode> enums_ = new Dictionary<string, EnumNode>();
        public List<MessageNode> nodes_ = new List<MessageNode>();

        public void ParseFile(string file)
        {
            StreamReader sr = new StreamReader(file);
            string data = sr.ReadToEnd();
            sr.Close();

            file_path_ = file;

            Token tk = new Token();
            List<TokenNode> tks = tk.Parse(data);
            Parse(tks);
        }

        // return the message serialize size
        public int GetSize()
        {
            int size = 0;
            for (int i = 0; i < nodes_.Count; i++)
            {

            }

            return size;
        }

        static int GetTypeSize(Token.eValueType type)
        {
            switch (type)
            {
                case Token.eValueType.e_bool:
                    return Marshal.SizeOf(typeof(bool));
                case Token.eValueType.e_byte:
                    return Marshal.SizeOf(typeof(byte));
                case Token.eValueType.e_fixed32:
                    return Marshal.SizeOf(typeof(int));
                case Token.eValueType.e_fixed64:
                    return Marshal.SizeOf(typeof(long));
                case Token.eValueType.e_float:
                    return Marshal.SizeOf(typeof(float));
                case Token.eValueType.e_int32:
                    return Marshal.SizeOf(typeof(int));
                case Token.eValueType.e_int64:
                    return Marshal.SizeOf(typeof(long));
                case Token.eValueType.e_uint32:
                    return Marshal.SizeOf(typeof(int));
                case Token.eValueType.e_uint64:
                    return Marshal.SizeOf(typeof(long));
                default:
                    throw new Exception("can not use");
            }
        }

        public void Parse(List<TokenNode> data)
        {
            int idx = 0;
            while (idx < data.Count)
            {
                TokenNode node = data[idx];

                if (node.type == TokenType.e_keyworld &&
                    (Token.eKeyWord)node.value == Token.eKeyWord.e_message)
                {
                    MessageNode msg_node = new MessageNode();
                    nodes_.Add(msg_node);

                    node = ReadNext(ref idx, data);
                    if (node.type != TokenType.e_value)
                        throw new Exception("not a name");
                    msg_node.name = node.raw_data;
                    node = ReadNext(ref idx, data);
                    if (node.type == TokenType.e_symbol &&
                        (Token.eSymbol)node.value == Token.eSymbol.e_left_brace)
                    {
                        while (idx < data.Count)
                        {
                            TokenNode check = ReadNext(ref idx, data);
                            if (check.type == TokenType.e_symbol &&
                                (Token.eSymbol)check.value == Token.eSymbol.e_right_brace)
                            {
                                idx++;
                                break;
                            }
                            else
                            {
                                // give back
                                idx--;
                                ValueNode val_node = ParseLine(ref idx, data, msg_node.name);
                                if (val_node != null)
                                {
                                    CheckEnum(val_node);
                                }
                                if (val_node != null)
                                    msg_node.nodes.Add(val_node);
                            }
                        }
                    }
                    else
                        throw new Exception("need {");


                }
                else if (CheckType(node, Token.eKeyWord.e_import))
                {
                    // must import other file
                    node = ReadNext(ref idx, data);
                    if (node.type != TokenType.e_string)
                        throw new Exception("need string");
                    ParseTree tree = new ParseTree();
                    tree.ParseFile(Path.Combine(Path.GetDirectoryName(file_path_), node.value.ToString()));
                    import_proto_.Add(tree);
                    if (CheckType(ReadNext(ref idx, data), Token.eSymbol.e_semicolon) == false)
                        throw new Exception("need ;");
                    idx++;

                }
                else if (CheckType(node, Token.eKeyWord.e_package))
                {
                    node = ReadNext(ref idx, data);
                    if (node.type == TokenType.e_value)
                        package_name_ = node.raw_data;
                    node = ReadNext(ref idx, data);
                    if (CheckType(node, Token.eSymbol.e_semicolon) == false)
                        throw new Exception("need ;");
                    idx++;       
                }
                else if (CheckType(node, Token.eKeyWord.e_option))
                {
                    ParseOption(ref idx, data);
                }
                else if (CheckType(node, Token.eKeyWord.e_enum))
                {
                    ParseEnum(ref idx, data, null);
                }
                else
                    throw new Exception("known: " + node.raw_data);
            }

            CheckReference();
        }

        void CheckEnum(ValueNode node)
        {
            foreach (EnumNode key in enums_.Values)
            {
                if (node.ref_name == key.enum_name)
                {
                    node.value_type = Token.eValueType.e_enum;
                }
            }
        }


        void CheckReference()
        {
            // check value refencnce
            for (int i = 0; i < nodes_.Count; i++)
            {
                MessageNode msg_node = nodes_[i];
                for (int j = 0; j < msg_node.nodes.Count; j++)
                {
                    if (msg_node.nodes[j].value_type == Token.eValueType.e_ref)
                    {
                        if (FindMessageName(msg_node.nodes[j].ref_name) == false)
                        {
                            Console.Write(msg_node.nodes[j].ref_name + msg_node.nodes[j].name);
                            throw new Exception("not define value");
                        }
                    }
                }
            }
        }

        public bool FindMessageName(string name)
        {
            // find in enums
            //if (enums_.Keys.Contains(name) == true)
            //    return true;
            foreach (EnumNode node in enums_.Values)
            {
                if (node.enums_.Keys.Contains(name) == true)
                    return true;
            }

            for (int i = 0; i < nodes_.Count; i++)
            {
                if (nodes_[i].name == name)
                    return true;
            }
            // find in import files
            for (int i = 0; i < import_proto_.Count; i++)
            {
                if (import_proto_[i].FindMessageName(name) == true)
                    return true;
            }
            return false;
        }

        TokenNode ReadNext(ref int idx, List<TokenNode> data)
        {
            idx++;
            if (idx < data.Count)
                return data[idx];
            else
                throw new Exception("not end");
        }

        ValueNode ParseLine(ref int idx, List<TokenNode> data, string msg_name)
        {
            ValueNode val_node = new ValueNode();
            TokenNode node = ReadNext(ref idx, data);
            if (node.type == TokenType.e_keyworld)
            {
                if ((Token.eKeyWord)node.value == Token.eKeyWord.e_optional ||
                    (Token.eKeyWord)node.value == Token.eKeyWord.e_repeated ||
                    (Token.eKeyWord)node.value == Token.eKeyWord.e_required)
                {
                    val_node.type = (Token.eKeyWord)node.value;
                    node = ReadNext(ref idx, data);

                    if (node.type == TokenType.e_value_type)
                        val_node.value_type = (Token.eValueType)node.value;

                    else if (node.type == TokenType.e_value) // reference
                    {
                        val_node.value_type = Token.eValueType.e_ref;
                        val_node.ref_name = node.raw_data;
                    }
                    else
                        throw new Exception(" ");

                    node = ReadNext(ref idx, data);
                    if (node.type != TokenType.e_string && 
                        node.type != TokenType.e_symbol && 
                        node.type != TokenType.e_number)
                    {
                        val_node.name = node.raw_data;
                        node = ReadNext(ref idx, data);
                        if (node.type == TokenType.e_symbol &&
                            (Token.eSymbol)node.value == Token.eSymbol.e_equal)
                        {
                            node = ReadNext(ref idx, data);
                            if (node.type == TokenType.e_number)
                            {
                                val_node.index = (int)node.value;
                                // check ; or []
                                node = ReadNext(ref idx, data);
                                if (node.type == TokenType.e_symbol)
                                {
                                    if ((Token.eSymbol)node.value == Token.eSymbol.e_semicolon)
                                    {
                                        return val_node; // end
                                    }
                                    else if ((Token.eSymbol)node.value == Token.eSymbol.e_left_brackets)
                                    {
                                        val_node.defult_value = ParseDefault(ref idx, data);
                                        if (CheckType(ReadNext(ref idx, data), Token.eSymbol.e_right_brackts) == false)
                                            throw new Exception("");
                                        if (CheckType(ReadNext(ref idx, data), Token.eSymbol.e_semicolon) == false)
                                            throw new Exception("");
                                        return val_node;
                                    }
                                    else
                                        throw new Exception("");
                                }
                                else
                                    throw new Exception("");
                            }
                            else
                                throw new Exception("");
                        }
                        else
                            throw new Exception("");
                    }
                    else
                        throw new Exception("");
                }
                else if (CheckType(node, Token.eKeyWord.e_enum))
                {
                    ParseEnum(ref idx, data, msg_name);
                    return null;
                }
                else
                    throw new Exception("unknow type");
            }
            else
                throw new Exception("");
        }

        //[default = 1]
        object ParseDefault(ref int idx, List<TokenNode> data)
        {
            TokenNode node = ReadNext(ref idx, data);
            if (node.type == TokenType.e_keyworld &&
                (Token.eKeyWord)node.value == Token.eKeyWord.e_default)
            {
                node = ReadNext(ref idx, data);
                if (node.type == TokenType.e_symbol &&
                    (Token.eSymbol)node.value == Token.eSymbol.e_equal)
                {
                    node = ReadNext(ref idx, data);
                    if (node.type == TokenType.e_number ||
                        node.type == TokenType.e_string)
                    {
                        return node.value;
                    }
                    else
                        throw new Exception("");
                }
                else
                    throw new Exception("");
            }
            else
                throw new Exception("not default");
        }

        void ParseEnum(ref int idx, List<TokenNode> data, string message_name)
        {
            TokenNode node = ReadNext(ref idx, data);
            if (node.type != TokenType.e_value)
                throw new Exception("need value");
            string name = node.raw_data;
            Dictionary<string, object> items = new Dictionary<string, object>();
            Dictionary<string, Dictionary<string, object>> inner_enum = new Dictionary<string, Dictionary<string, object>>();
            EnumNode enum_node = new EnumNode();
            enum_node.message_name = message_name;
            enum_node.enum_name = name;
            enum_node.enums_ = inner_enum;
            ExceptionType(ReadNext(ref idx, data), Token.eSymbol.e_left_brace, "need {");
            while (idx < data.Count)
            {
                node = ReadNext(ref idx, data);
                if (CheckType(node, Token.eSymbol.e_right_brace))
                    break;
                if (node.type != TokenType.e_value)
                    throw new Exception("need value");
                string item_name = node.raw_data;
                ExceptionType(ReadNext(ref idx, data), Token.eSymbol.e_equal, "need =");
                node = ReadNext(ref idx, data);
                if (node.type != TokenType.e_number)
                    throw new Exception("need number");
                int v = (int)node.value;

                items.Add(item_name, v);
                CheckType(ReadNext(ref idx, data), Token.eSymbol.e_semicolon);
            }
            inner_enum.Add(name, items);
            enums_.Add(message_name, enum_node);
        }

        void ParseOption(ref int idx, List<TokenNode> data)
        {
            TokenNode node = ReadNext(ref idx, data);
            if (node.type != TokenType.e_value)
                throw new Exception("need value");
            string key = node.raw_data;
            if (CheckType(ReadNext(ref idx, data), Token.eSymbol.e_equal) == false)
                throw new Exception("need =");
            node = ReadNext(ref idx, data);
            if (node.type != TokenType.e_string)
                throw new Exception("need string");
            string val = node.raw_data;
            opions_.Add(key, val);

            if (CheckType(ReadNext(ref idx, data), Token.eSymbol.e_semicolon) == false)
                throw new Exception("need ;");
            idx++;
        }

        bool CheckType(TokenNode node, Token.eSymbol sym)
        {
            return (node.type == TokenType.e_symbol && (Token.eSymbol)node.value == sym);
        }

        bool CheckType(TokenNode node, Token.eKeyWord kw)
        {
            return (node.type == TokenType.e_keyworld && (Token.eKeyWord)node.value == kw);
        }

        void ExceptionType(TokenNode node, Token.eSymbol sym, string msg)
        {
            if (CheckType(node, sym) == false)
                throw new Exception(msg);
        }

        void ExcetoionType(TokenNode node, Token.eKeyWord kw, string msg)
        {
            if (CheckType(node, kw) == false)
                throw new Exception(msg);
        }

        
    }
}
