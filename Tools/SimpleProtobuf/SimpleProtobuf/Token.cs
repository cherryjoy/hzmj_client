using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtobuf
{
    enum TokenType
    {
        e_keyworld,
        e_value_type,
        e_symbol,
        e_value,
        e_string,
        e_number,
        e_enter,
        e_expression,   // a expression
        e_enum_value,
        e_unknown
    }

    class TokenNode
    {
        public TokenType type;
        public string raw_data;
        public object value;
        public int line;

        public string ToString() {
            return "type: " + type + "   raw_data: " + raw_data + "   value: " + string.Format("{0}", value) + "  line: " + line + "\n";   
        }
    }

    class Token
    {
        public static string[] mKeyWord = 
        {
            "message",
            "required",
            "repeated",
            "optional",
            "option",
            "import",
            "default",
            "package",
            "enum"
        };

        public enum eKeyWord
        {
            e_message,
            e_required,
            e_repeated,
            e_optional,
            e_option,
            e_import,
            e_default,
            e_package,
            e_enum
        };

        public static string[] mValueType =
        { 
            "int32",
            "uint32",
            "uint64",
            "string",
            "float",
            "fixed32",
            "fixed64",
            "int64",
            "bytes",
            "bool",
            "longid"
        };

        public enum eValueType
        {
            e_int32,
            e_uint32,
            e_uint64,
            e_string,
            e_float,
            e_fixed32,
            e_fixed64,
            e_int64,
            e_byte,
            e_bool,
            e_longid,
            //============
            e_ref,
            e_enum,
        };

        public static string[] mSymbol =
        {
            ";",
            "=",
            "{",
            "}",
            "[",
            "]",
        };

        public enum eSymbol
        {
            e_semicolon,
            e_equal,
            e_left_brace,
            e_right_brace,
            e_left_brackets,
            e_right_brackts
        }

        public static string Enter = "\r\n";

        List<TokenNode> ParseExpression(StringBuilder sb)
        {            
            ExpressionToken exp = new ExpressionToken();
            List<TokenNode> n = exp.Parse(sb);
            for (int i = 0; i < n.Count; i++)
                CheckSymbol(n[i]);
            return n;
        }

        // number type
        // 0 int
        // 1 float
        // 2 double
        public List<TokenNode> Parse(string data)
        {
            List<TokenNode> nodes = new List<TokenNode>();
            int idx = 0;
            bool in_token = false;
            StringBuilder sb = new StringBuilder();
            int line = 0;

            while (idx < data.Length)
            {
                char c = data[idx];
                if (IsEmptyChar(c) == true)
                {
                    if (in_token == true)
                    {
                        nodes.AddRange(ParseExpression(sb));
                        in_token = false;
                        sb.Length = 0;
                    }
                    else
                        idx++;
                }
                else if (IsEnterChar(c) == true)
                {
                    if (in_token == true)
                    {
                        nodes.AddRange(ParseExpression(sb));
                        in_token = false;
                        sb.Length = 0;
                    }
                    
                    idx++;
                    line++;
                }
                else if (c == '\"')
                {
                    if (in_token == true)
                    {
                        // cut down
                        in_token = false;
                        nodes.AddRange(ParseExpression(sb));
                    }

                    // read to " end
                    sb.Length = 0;
                    idx++;
                    while (idx < data.Length)
                    {
                        char n_c = data[idx];
                        if (n_c == '\"')
                        {
                            break;
                        }
                        else
                        {
                            sb.Append(n_c);
                            idx++;
                        }
                    }

                    TokenNode node = new TokenNode();
                    node.type = TokenType.e_string;
                    node.raw_data = sb.ToString();
                    node.value = node.raw_data;
                    nodes.Add(node);
                    sb.Length = 0;
                    idx++;

                }
                else if (c == '/' && idx < data.Length - 1 && data[idx + 1] == '/')
                {
                    while (IsEnterChar(c) == false && idx < data.Length - 1) // read to enter
                    {
                        c = data[++idx];
                    }
                    idx++;
                }
                else if (c == '/' && idx < data.Length - 1 && data[idx + 1] == '*')
                {
                    while ((c == '*' && data[idx + 1] == '/' && idx < data.Length - 1) == false) // read to */
                    {
                        c = data[++idx];
                        if (IsEnterChar(c) == true)
                            line++;
                    }
                    idx += 2;
                }
                else
                {
                    in_token = true;
                    sb.Append(c);
                    idx++;
                }
            }
            if (sb.Length > 0)
            {
                nodes.AddRange(ParseExpression(sb));
            }

            return nodes;
        }

        void CheckSymbol(TokenNode node)
        {
            if (node.type == TokenType.e_symbol)
            {
                for (int i = 0; i < mSymbol.Length; i++)
                {
                    if (mSymbol[i] == node.raw_data)
                    {
                        node.value = i;                  
                        return;
                    }
                }

                throw new Exception("symbol unknown");
            }
        }

        bool IsEmptyChar(char character)
        {
            if (character == ' ' || character == '\t' || character == '\r')
                return true;
            else
                return false;
        }

        bool IsEnterChar(char character)
        {
            return character == '\n';
        }
    }
}
