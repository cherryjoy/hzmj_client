using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtobuf
{
    class ExpressionToken
    {
        // 0 normal
        // 1 number
        // 2 letter
        // 3 symbol
        // 4 float number

        // 1.0
        // a.b
        // a12
        // .12e
        // a+12
        // b(a)

        enum LetterType
        {
            e_number,
            e_letter,
            e_symbol
        }

        public List<TokenNode> Parse(StringBuilder sb)
        {
            List<TokenNode> nodes = new List<TokenNode>();

            int idx = 0;
            int state = 0;
            StringBuilder expr = new StringBuilder();
            while (idx < sb.Length)
            {
                char c = sb[idx];
                LetterType type = GetType(c);
                switch (state)
                {
                    case 0:
                        if (c == '.' || c == '-')
                            state = 4;
                        else if (type == LetterType.e_letter)
                            state = 2;
                        else if (type == LetterType.e_number)
                            state = 1;
                        else if (type == LetterType.e_symbol)
                            state = 3;

                        expr.Append(c);
                        idx++;
                        break;
                    case 1:

                        if (type == LetterType.e_number)
                        {
                            expr.Append(c);
                            idx++;
                        }
                        else if (c == '.')
                        {
                            expr.Append(c);
                            state = 4;
                            idx++;
                        }
                        else if (type == LetterType.e_letter)
                        {
                            throw new Exception("error number");
                        }
                        else if (type == LetterType.e_symbol)
                        {
                            TokenNode node = new TokenNode();
                            node.type = TokenType.e_number;
                            node.raw_data = expr.ToString();
                            node.value = int.Parse(node.raw_data);
                            nodes.Add(node);

                            expr.Length = 0;
                            state = 0; // back
                        }
                        else
                            throw new Exception("error number");
                        break;
                    case 2:
                        if (type == LetterType.e_letter || type == LetterType.e_number)
                        {
                            expr.Append(c);
                            idx++;
                        }
                        else if (type == LetterType.e_symbol)
                        {
                            nodes.Add(ParseValue(expr.ToString()));
                            expr.Length = 0;
                            state = 0; // back
                        }
                        break;
                    case 3:
                        if (type == LetterType.e_letter || type == LetterType.e_number)
                        {
                            nodes.AddRange(ParseSymbols(expr));

                            expr.Length = 0;
                            state = 0;
                        }
                        else if (type == LetterType.e_symbol)
                        {
                            idx++;
                            expr.Append(c);
                        }
                        break;
                    case 4:
                        if (c == 'e')
                        {
                            TokenNode node = new TokenNode();
                            node.type = TokenType.e_number;
                            node.raw_data = expr.ToString();
                            node.value = float.Parse(node.raw_data);
                            nodes.Add(node);

                            expr.Length = 0;
                            state = 0;
                            idx++;
                        }
                        else if (type == LetterType.e_number)
                        {
                            idx++;
                            expr.Append(c);
                        }
                        else if (type == LetterType.e_symbol)
                        {
                            TokenNode node = new TokenNode();
                            node.type = TokenType.e_number;
                            node.raw_data = expr.ToString();
                            node.value = float.Parse(node.raw_data);
                            nodes.Add(node);

                            expr.Length = 0;
                            state = 0;
                        }
                        else
                            throw new Exception("float number error");
                        break;
                }
            }

            if (expr.Length > 0)
            {
                TokenNode node = new TokenNode();
                node.raw_data = expr.ToString();
                if (state == 1)
                {
                    node.type = TokenType.e_number;
                    node.value = int.Parse(node.raw_data);
                    nodes.Add(node);
                }
                else if (state == 2)
                {
                    node = ParseValue(expr.ToString());
                    nodes.Add(node);
                }
                else if (state == 3)
                {
                    nodes.AddRange(ParseSymbols(expr));
                }
                else if (state == 4)
                {
                    node.type = TokenType.e_number;
                    node.value = float.Parse(node.raw_data);
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        LetterType GetType(char c)
        {
            if (char.IsDigit(c) == true)
                return LetterType.e_number;
            if (char.IsLetter(c) == true)
                return LetterType.e_letter;
            if (c == '_')
                return LetterType.e_letter;
            return LetterType.e_symbol;
        }

        List<TokenNode> ParseSymbols(StringBuilder sb)
        {
            List<TokenNode> nodes = new List<TokenNode>();
            string s = sb.ToString();
            for (int i = 0; i < Token.mSymbol.Length; i++)
            {
                if (s == Token.mSymbol[i])
                {
                    TokenNode node = new TokenNode();
                    node.raw_data = s;
                    node.type = TokenType.e_symbol;
                    node.value = i;

                    nodes.Add(node);
                    return nodes;
                }
            }

            for (int i = 0; i < sb.Length; i++)
            {
                int j = 0;
                for (; j < Token.mSymbol.Length; j++)
                {
                    if (sb[i].ToString() == Token.mSymbol[j])
                    {
                        TokenNode node = new TokenNode();
                        node.raw_data = sb[i].ToString();
                        node.value = i;
                        node.type = TokenType.e_symbol;

                        nodes.Add(node);
                        break;
                    }
                }

                if (j == Token.mSymbol.Length)
                    throw new Exception("unknown symbol:" + sb.ToString());
            }

            return nodes;
        }

        TokenNode ParseValue(string value)
        {
            TokenNode node = new TokenNode();
            node.raw_data = value;
            for (int i = 0; i < Token.mKeyWord.Length; i++)
            {
                if (Token.mKeyWord[i] == value)
                {
                    node.value = i;
                    node.type = TokenType.e_keyworld;
                    return node;
                }
            }

            for (int i = 0; i < Token.mValueType.Length; i++)
            {
                if (Token.mValueType[i] == value)
                {
                    node.value = i;
                    node.type = TokenType.e_value_type;

                    return node;
                }
            }

            node.type = TokenType.e_value;
            return node;
        }

        
    }
}
