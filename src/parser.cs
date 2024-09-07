using System;
using System.Collections.Generic;
using VSharp;

namespace VSharp
{
    public abstract class ASTNode { }

    public class ProgramNode : ASTNode
    {
        public List<ASTNode> Statements { get; }

        public ProgramNode()
        {
            Statements = new List<ASTNode>();
        }
    }

    public class ArgNode : ASTNode
    {
        public List<string> Names { get; set; }

        public ArgNode()
        {
            Names = new List<string>();
        }
    }

    public class FuncCallNode : ASTNode
    {
        public List<ASTNode> Args { get; set; }
        public string FuncName { get; set; }

        public FuncCallNode()
        {
            Args = new List<ASTNode>();
        }
    }

    public class FuncStatementNode : ASTNode
    {
        public string Name { get; set; }
        public ArgNode Args { get; set; }
        public BlockNode Block { get; set; }
        public Dictionary<string, object> Vars { get; set; }

        public FuncStatementNode()
        {
            Args = new ArgNode();
            Block = new BlockNode();
        }
    }

    public class BlockNode : ASTNode
    {
        public List<ASTNode> Statements { get; }

        public BlockNode()
        {
            Statements = new List<ASTNode>();
        }

        public BlockNode(List<ASTNode> statements)
        {
            Statements = statements;
        }
    }

    public class ForegroundColorStatementNode : ASTNode
    {
        public ASTNode ColorName { get; set; }

        public ForegroundColorStatementNode()
        {
        }
    }

    public class IfNode : ASTNode
    {
        public ASTNode Condition { get; set; }
        public BlockNode TrueBlock { get; set; }
        public BlockNode FalseBlock { get; set; }

        public IfNode()
        {
            TrueBlock = new BlockNode();
            FalseBlock = new BlockNode();
        }
    }

    public class WhileStatementNode : ASTNode
    {
        public ASTNode Condition { get; set; }
        public BlockNode TrueBlock { get; set; }

        public WhileStatementNode()
        {
            TrueBlock = new BlockNode();
        }
    }

    public class SetStatementNode : ASTNode
    {
        public string VariableName { get; }
        public ASTNode Expression { get; }

        public SetStatementNode(string variableName, ASTNode expression)
        {
            VariableName = variableName;
            Expression = expression;
        }
    }

    public class PrintStatementNode : ASTNode
    {
        public ASTNode Expression { get; }

        public PrintStatementNode(ASTNode expression)
        {
            Expression = expression;
        }
    }

    public class ConvertToIntStatementNode : ASTNode
    {
        public ASTNode Expr { get; set; }
        public string VarName { get; set; }

        public ConvertToIntStatementNode()
        {
        }
    }

    public class PrintlnStatementNode : ASTNode
    {
        public ASTNode Expression { get; }

        public PrintlnStatementNode(ASTNode expression)
        {
            Expression = expression;
        }
    }

    public class InputStatementNode : ASTNode
    {
        public string VarName { get; }

        public InputStatementNode(string varName)
        {
            VarName = varName;
        }
    }

    public class LiteralNode : ASTNode
    {
        public string Value { get; }

        public LiteralNode(string value)
        {
            Value = value;
        }
    }

    public class IdentifierNode : ASTNode
    {
        public string Name { get; }

        public IdentifierNode(string name)
        {
            Name = name;
        }
    }

    public class BinaryOperationNode : ASTNode
    {
        public ASTNode Left { get; }
        public string Operator { get; }
        public ASTNode Right { get; }

        public BinaryOperationNode(ASTNode left, string operatorSymbol, ASTNode right)
        {
            Left = left;
            Operator = operatorSymbol;
            Right = right;
        }
    }

    public class LogicalNode : ASTNode
    {
        public ASTNode Left { get; set; }
        public Token Operator { get; set; }
        public ASTNode Right { get; set; }

        public LogicalNode(ASTNode left, Token operatorSymbol, ASTNode right)
        {
            Left = left;
            Operator = operatorSymbol;
            Right = right;
        }
    }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        public ProgramNode Parse()
        {
            ProgramNode program = new ProgramNode();

            while (!IsAtEnd())
            {
                program.Statements.Add(ParseStatement());
            }

            return program;
        }

        private ASTNode ParseStatement()
        {
            Token current = Peek();

            switch (current.Type)
            {
                case TokenType.KeywordSet:
                    return ParseSetStatement();
                case TokenType.KeywordPrint:
                    return ParsePrintStatement();
                case TokenType.KeywordInput:
                    return ParseInputStatement();
                case TokenType.KeywordIf:
                    return ParseIfStatement();
                case TokenType.KeywordPrintln:
                    return ParsePrintlnStatement();
                case TokenType.KeywordWhile:
                    return ParseWhileStatement();
                case TokenType.KeywordConvertToInt:
                    return ParseConvertToIntStatement();
                case TokenType.KeywordForegroundColor:
                    return ParseForegroundColorStatement();
                case TokenType.KeywordFunc:
                    return ParseFuncStatement();
                case TokenType.Identifier:
                    return ParseFuncCall();
                default:
                    _position++;
                    return null;
            }
        }

        private FuncCallNode ParseFuncCall()
        {
            FuncCallNode funcCall = new FuncCallNode();
            var name = Consume(TokenType.Identifier, "Expected function name.");
            Consume(TokenType.LeftParen, "Expected '(' after function name.");

            while (Peek().Type != TokenType.RightParen)
            {
                funcCall.Args.Add(ParseExpression());

                if (Peek().Type == TokenType.Comma)
                {
                    Consume(TokenType.Comma, "Expected ',' after argument.");
                }
            }

            Consume(TokenType.RightParen, "Expected ')' after arguments.");
            funcCall.FuncName = name.Value;
            return funcCall;
        }

        private FuncStatementNode ParseFuncStatement()
        {
            Consume(TokenType.KeywordFunc, "Expected 'func' keyword");
            var name = Consume(TokenType.Identifier, "Expected function name");
            ArgNode args = ParseArgs();
            BlockNode block = ParseBlockNode();
            FuncStatementNode func = new FuncStatementNode
            {
                Args = args,
                Block = block,
                Name = name.Value
            };
            return func;
        }

        private ArgNode ParseArgs()
        {
            ArgNode arg = new ArgNode();
            Consume(TokenType.LeftParen, "Expected '(' after 'func'");
            while (Peek().Type != TokenType.RightParen)
            {
                if (Peek().Type == TokenType.Comma)
                {
                    Consume(TokenType.Comma, "Expected ',' after argument");
                }
                if (Peek().Type == TokenType.Identifier)
                {
                    arg.Names.Add(Peek().Value);
                    _position++;
                }
            }
            Consume(TokenType.RightParen, "Expected ')' after arguments");

            return arg;
        }

        private ForegroundColorStatementNode ParseForegroundColorStatement()
        {
            Consume(TokenType.KeywordForegroundColor, "Expected 'color' keyword.");
            Consume(TokenType.LeftParen, "Expected '(' after 'color'.");
            var colorName = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after color name.");
            return new ForegroundColorStatementNode { ColorName = colorName };
        }

        private ConvertToIntStatementNode ParseConvertToIntStatement()
        {
            Consume(TokenType.KeywordConvertToInt, "Expected 'ConvertToInt' keyword");
            Consume(TokenType.LeftParen, "Expected '(' after 'ConvertToInt'");
            var expr = ParseExpression();
            Consume(TokenType.Comma, "Expected ',' after expression");
            var name = Consume(TokenType.Identifier, "Expected variable name");
            Consume(TokenType.RightParen, "Expected ')' after variable name");
            return new ConvertToIntStatementNode
            {
                Expr = expr,
                VarName = name.Value
            };
        }

        private WhileStatementNode ParseWhileStatement()
        {
            Consume(TokenType.KeywordWhile, "Expected 'while' keyword");
            Consume(TokenType.LeftParen, "Expected '(' after 'while'");
            var condition = ParseComparison();
            Consume(TokenType.RightParen, "Expected ')' after condition");
            var trueBlock = ParseBlockNode();
            return new WhileStatementNode
            {
                Condition = condition,
                TrueBlock = trueBlock
            };
        }

        private IfNode ParseIfStatement()
        {
            Consume(TokenType.KeywordIf, "Expected 'if' keyword");
            Consume(TokenType.LeftParen, "Expected '(' after 'if'");
            var condition = ParseComparison();
            Consume(TokenType.RightParen, "Expected ')' after condition");
            var trueBlock = ParseBlockNode();
            var ifNode = new IfNode
            {
                Condition = condition,
                TrueBlock = trueBlock
            };
            if (Peek().Type == TokenType.KeywordElse)
            {
                Consume(TokenType.KeywordElse, "Expected 'else' keyword");
                var falseBlock = ParseBlockNode();
                ifNode.FalseBlock = falseBlock;
            }
            return ifNode;
        }

        private ASTNode ParseComparison()
        {
            ASTNode left = ParseExpression();

            if (Peek().Type == TokenType.Greater || Peek().Type == TokenType.Less ||
                Peek().Type == TokenType.Equal || Peek().Type == TokenType.NotEqual ||
                Peek().Type == TokenType.GreaterEqual || Peek().Type == TokenType.LessEqual)
            {
                Token comparisonOp = NextToken();
                ASTNode right = ParseExpression();
                left = new BinaryOperationNode(left, comparisonOp.Value, right);
            }

            return left;
        }

        private BlockNode ParseBlockNode()
        {
            List<ASTNode> statements = new List<ASTNode>();

            Consume(TokenType.LeftBrace, "Expected '{'");

            while (!IsAtEnd() && Peek().Type != TokenType.RightBrace)
            {
                statements.Add(ParseStatement());
            }

            Consume(TokenType.RightBrace, "Expected '}'");

            return new BlockNode(statements);
        }

        private InputStatementNode ParseInputStatement()
        {
            Consume(TokenType.KeywordInput, "Expected 'input' keyword");
            Consume(TokenType.LeftParen, "Expected '(' after 'input'");
            Token id = Consume(TokenType.Identifier, "Expected variable name");
            Consume(TokenType.RightParen, "Expected ')' after variable name");
            return new InputStatementNode(id.Value);
        }

        private SetStatementNode ParseSetStatement()
        {
            Consume(TokenType.KeywordSet, "Expected 'set' keyword.");
            Token identifier = Consume(TokenType.Identifier, "Expected variable name.");
            Consume(TokenType.Assignment, "Expected '=' after variable name.");
            ASTNode expression = ParseExpression();
            return new SetStatementNode(identifier.Value, expression);
        }

        private PrintStatementNode ParsePrintStatement()
        {
            Consume(TokenType.KeywordPrint, "Expected 'print' keyword.");
            Consume(TokenType.LeftParen, "Expected '(' after 'print'.");
            ASTNode expression = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after expression.");
            return new PrintStatementNode(expression);
        }

        private PrintlnStatementNode ParsePrintlnStatement()
        {
            Consume(TokenType.KeywordPrintln, "Expected 'println' keyword.");
            Consume(TokenType.LeftParen, "Expected '(' after 'println' keyword");
            ASTNode expr = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after expression");
            return new PrintlnStatementNode(expr);
        }

        private ASTNode ParseExpression()
        {
            return ParseTerm();
        }

        private ASTNode ParseTerm()
        {
            ASTNode node = ParseFactor();

            while (Peek().Type == TokenType.Operator && (Peek().Value == "+" || Peek().Value == "-"))
            {
                Token operatorToken = NextToken();
                ASTNode right = ParseFactor();
                node = new BinaryOperationNode(node, operatorToken.Value, right);
            }

            return node;
        }

        private ASTNode ParseFactor()
        {
            ASTNode node = ParsePrimary();

            while (Peek().Type == TokenType.Operator && (Peek().Value == "*" || Peek().Value == "/"))
            {
                Token operatorToken = NextToken();
                ASTNode right = ParsePrimary();
                node = new BinaryOperationNode(node, operatorToken.Value, right);
            }

            return node;
        }

        private ASTNode ParsePrimary()
        {
            Token current = Peek();

            switch (current.Type)
            {
                case TokenType.IntegerLiteral:
                case TokenType.FloatLiteral:
                case TokenType.StringLiteral:
                    NextToken();
                    return new LiteralNode(current.Value);
                case TokenType.Identifier:
                    NextToken();
                    return new IdentifierNode(current.Value);
                default:
                    throw new Exception($"Unexpected token: {current}");
            }
        }

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Peek().Type == type)
            {
                return NextToken();
            }

            throw new Exception(errorMessage);
        }

        private Token Peek()
        {
            if (IsAtEnd()) return new Token(TokenType.EndOfInput, "");
            return _tokens[_position];
        }

        private Token NextToken()
        {
            if (!IsAtEnd()) _position++;
            return _tokens[_position - 1];
        }

        private bool IsAtEnd()
        {
            return _position >= _tokens.Count;
        }
    }
}
