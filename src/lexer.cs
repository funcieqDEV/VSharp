using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VSharp
{

    public enum TokenType
    {
        KeywordSet,
        KeywordPrint,
        KeywordPrintln,
        KeywordInput,
        KeywordIf,
        KeywordElse,
        KeywordConvertToInt,
        KeywordWhile,
        KeywordForegroundColor,
        KeywordFunc,
        Identifier,
        IntegerLiteral,
        FloatLiteral,
        StringLiteral,
        Operator,
        Assignment,
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        LogicalOr,
        LogicalAnd,
        Less,
        LessEqual,
        Greater,
        GreaterEqual,
        Equal,
        NotEqual,
        Comma,
        EndOfInput
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"Token({Type}, '{Value}')";
        }
    }


    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            { "set", TokenType.KeywordSet },
            { "print", TokenType.KeywordPrint },
            { "input", TokenType.KeywordInput },
            { "if", TokenType.KeywordIf },
            { "else", TokenType.KeywordElse },
            { "or", TokenType.LogicalOr },
            { "and", TokenType.LogicalAnd },
            { "println", TokenType.KeywordPrintln},
            { "while", TokenType.KeywordWhile},
            { "ConvertToInt", TokenType.KeywordConvertToInt},
            { "ForegroundColor", TokenType.KeywordForegroundColor},
            { "func", TokenType.KeywordFunc}
        };

        public Lexer(string input)
        {
            _input = input;
            _position = 0;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char currentChar = _input[_position];

                if (char.IsWhiteSpace(currentChar))
                {
                    _position++;
                }
                else if (char.IsLetter(currentChar))
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                }
                else if (char.IsDigit(currentChar))
                {
                    tokens.Add(ReadNumber());
                }
                else if (currentChar == '"')
                {
                    tokens.Add(ReadString());
                }
                else if (currentChar == '=')
                {
                    if (LookAhead() == '=')
                    {
                        _position += 2;
                        tokens.Add(new Token(TokenType.Equal, "=="));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Assignment, "="));
                        _position++;
                    }
                }
                else if (currentChar == '!')
                {
                    if (LookAhead() == '=')
                    {
                        _position += 2;
                        tokens.Add(new Token(TokenType.NotEqual, "!="));
                    }
                    else
                    {
                        throw new Exception($"Unexpected character: {currentChar}");
                    }
                }
                else if (currentChar == ',')
                {
                    _position++;
                    tokens.Add(new Token(TokenType.Comma, ","));
                }
                else if (currentChar == '<')
                {
                    if (LookAhead() == '=')
                    {
                        _position += 2;
                        tokens.Add(new Token(TokenType.LessEqual, "<="));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Less, "<"));
                        _position++;
                    }
                }
                else if (currentChar == '/')
                {
                    if (LookAhead() == '/')
                    {
                        
                        while (_position < _input.Length && _input[_position] != '\n')
                        {
                            _position++;
                        }
                        continue; 
                    }
                }
                else if (currentChar == '>')
                {
                    if (LookAhead() == '=')
                    {
                        _position += 2;
                        tokens.Add(new Token(TokenType.GreaterEqual, ">="));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Greater, ">"));
                        _position++;
                    }
                }
                else if (IsOperator(currentChar))
                {
                    tokens.Add(new Token(TokenType.Operator, currentChar.ToString()));
                    _position++;
                }
                else if (currentChar == '(')
                {
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    _position++;
                }
                else if (currentChar == ')')
                {
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    _position++;
                }
                else if (currentChar == '{')
                {
                    tokens.Add(new Token(TokenType.LeftBrace, "{"));
                    _position++;
                }
                else if (currentChar == '}')
                {
                    tokens.Add(new Token(TokenType.RightBrace, "}"));
                    _position++;
                }
                else
                {
                    throw new Exception($"Unexpected character: {currentChar}");
                }
            }

            tokens.Add(new Token(TokenType.EndOfInput, ""));
            return tokens;
        }

        private char LookAhead()
        {
            if (_position + 1 < _input.Length)
            {
                return _input[_position + 1];
            }
            return '\0'; 
        }

        private bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/';
        }

        private Token ReadIdentifierOrKeyword()
        {
            int start = _position;
            while (_position < _input.Length && char.IsLetter(_input[_position]))
            {
                _position++;
            }

            string value = _input.Substring(start, _position - start);
            if (Keywords.ContainsKey(value))
            {
                return new Token(Keywords[value], value);
            }

            return new Token(TokenType.Identifier, value);
        }

        private Token ReadNumber()
        {
            int start = _position;
            bool isFloat = false;

            while (_position < _input.Length && (char.IsDigit(_input[_position]) || _input[_position] == '.'))
            {
                if (_input[_position] == '.')
                {
                    if (isFloat)
                    {
                        throw new Exception("Invalid number format: multiple decimal points");
                    }
                    isFloat = true;
                }
                _position++;
            }

            string value = _input.Substring(start, _position - start);
            return isFloat ? new Token(TokenType.FloatLiteral, value) : new Token(TokenType.IntegerLiteral, value);
        }

        private Token ReadString()
        {
            int start = ++_position; 
            while (_position < _input.Length && _input[_position] != '"')
            {
                _position++;
            }

            if (_position >= _input.Length)
            {
                throw new Exception("Unterminated string literal");
            }

            string value = _input.Substring(start, _position - start);
            _position++; 
            return new Token(TokenType.StringLiteral, value);
        }
    }
}