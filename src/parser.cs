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

    public class argNode : ASTNode
    {
        public List<string> names { get; set; }
        public argNode()
        {
            names = new List<string>();
        }


    }


    public class FuncCallNode : ASTNode
    {
        
        public List<ASTNode> args { get; set; }
        public string funcName { get; set; }

        public FuncCallNode()
        {
            args = new List<ASTNode>();
        }
    }
    public class FuncStatementNode : ASTNode
    {

        public string name { get; set; }
        public argNode args { get; set; }
        public BlockNode Block { get; set; }
        public Dictionary<string,object> _vars { get; set; }

        public FuncStatementNode()
        {
            args = new argNode();
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
        public BlockNode(List<ASTNode> stmts)
        {
            Statements = stmts;
        }
    }

    public class ForgroundColorStatementNode : ASTNode
    {
        public ASTNode ColorName { get; set; }

        public ForgroundColorStatementNode()
        {

        }
    }

    public class IfNode : ASTNode
    {
        public List<LogicalNode> Conditions { get; set; }
        public BlockNode TrueBlock { get; set; }
        public BlockNode FalseBlock { get; set; }

        public IfNode()
        {
            TrueBlock = new BlockNode();
            FalseBlock = new BlockNode();
            Conditions = new List<LogicalNode>();
        }
    }

    public class WhileStatementNode : ASTNode
    {
        public List<LogicalNode> Conditions { get; set; }
        public BlockNode TrueBlock { get; set; }

        public WhileStatementNode()
        {
            Conditions = new List<LogicalNode>();
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

    public class ConvertToIntStatamentNode : ASTNode
    {
        public ASTNode Expr { get; set; }
        public string VarName { get; set; }

        public ConvertToIntStatamentNode()
        {

        }
    }

    public class PrintlnStatementNode : ASTNode
    {
        public ASTNode Expression { get; }
        public PrintlnStatementNode(ASTNode expr)
        {
            Expression = expr;
        }
    }

    public class InputStatementNode : ASTNode
    {
        public string VarName { get; }

        public InputStatementNode(string name)
        {
            VarName = name;
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
                    // throw new Exception($"Unexpected token: {current}");
            }
        }

        private FuncCallNode ParseFuncCall()
        {
            FuncCallNode funcCall = new FuncCallNode();
            var name = Consume(TokenType.Identifier, "Expected function name.");
            Consume(TokenType.LeftParen, "Expected '(' after function name.");

            
            while (Peek().Type != TokenType.RightParen)
            {
                
                funcCall.args.Add(ParseExpression());

                
                if (Peek().Type == TokenType.Comma)
                {
                    Consume(TokenType.Comma, "Expected ',' after argument.");
                }
            }

            Consume(TokenType.RightParen, "Expected ')' after arguments.");
            funcCall.funcName = name.Value;
            return funcCall;
        }

        private FuncStatementNode ParseFuncStatement()
        {
            FuncStatementNode funcStatement = new FuncStatementNode();
            Consume(TokenType.KeywordFunc, "Expected 'func' keyword");
            var name = Consume(TokenType.Identifier, "Expected function name");
            argNode args = new argNode();
            args = ParseArgs();
            BlockNode block = new BlockNode();
            block = ParseBlockNode();
            FuncStatementNode func = new FuncStatementNode();
            func.args = args;
            func.Block = block;
            func.name = name.Value;
            return func;
        }

        private argNode ParseArgs()
        {
            argNode arg = new argNode();
            Consume(TokenType.LeftParen, "Expected '(' after 'func' ");
            while (Peek().Type != TokenType.RightParen)
            {

                if (Peek().Type == TokenType.Comma)
                {
                    Consume(TokenType.Comma, "Expected ',' after argument");
                }
                if (Peek().Type == TokenType.Identifier)
                {

                    arg.names.Add(Peek().Value);

                    _position++;
                }


            }
            Consume(TokenType.RightParen, "Expected ')' after arguments");
            
            return arg;
        }
        private ForgroundColorStatementNode ParseForegroundColorStatement()
        {
            Consume(TokenType.KeywordForegroundColor, "Expected 'color' keyword.");
            Consume(TokenType.LeftParen, "Expected '(' after 'color'.");
            var colorName = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after color name.");
            return new ForgroundColorStatementNode() { ColorName = colorName };
        }

        private ConvertToIntStatamentNode ParseConvertToIntStatement()
        {
            Consume(TokenType.KeywordConvertToInt, "Expected 'ConvertToInt' keyword");
            Consume(TokenType.LeftParen, "Expected '(' after 'ConvertToInt'");
            var expr = ParseExpression();
            Consume(TokenType.Comma, "Expected ',' after expression");
            var name = Consume(TokenType.Identifier, "Expected variable name");
            Consume(TokenType.RightParen, "Expexted ')' after variable name");
            ConvertToIntStatamentNode cti = new ConvertToIntStatamentNode();
            cti.Expr = expr;
            cti.VarName = name.Value;
            return cti;
        }

        private WhileStatementNode ParseWhileStatement()
        {
            Consume(TokenType.KeywordWhile, "Expected 'while' keyword");
            Consume(TokenType.LeftParen, "Expected '(' after 'while'");
            var conditions = ParseLogicalNode();
            Consume(TokenType.RightParen, "Expected ')' after conditions");
            var Block = ParseBlockNode();
            WhileStatementNode _while = new WhileStatementNode();
            _while.Conditions = conditions;
            _while.TrueBlock = Block;
            return _while;
        }

        private IfNode ParseIfStatement()
        {
            Consume(TokenType.KeywordIf, "Expected 'if' keyword");
            Consume(TokenType.LeftParen, "Expected '(' after 'if'");
            var conditions = ParseLogicalNode();
            Consume(TokenType.RightParen, "Expected ')' after condition");
            var TrueBlock = ParseBlockNode();
            IfNode _if = new IfNode();
            _if.Conditions = conditions;
            _if.TrueBlock = TrueBlock;
            if (Peek().Type == TokenType.KeywordElse){
                Consume(TokenType.KeywordElse,"Expected 'else' keyword");
                var FalseBlock = ParseBlockNode();
                _if.FalseBlock = FalseBlock;
            }
            return _if;
        }
        private List<LogicalNode> ParseLogicalNode()
        {
            List<LogicalNode> conditions = new List<LogicalNode>();
            ASTNode left = ParseComparison();

            while (Peek().Type == TokenType.LogicalAnd || Peek().Type == TokenType.LogicalOr)
            {
                Token logicalOp = NextToken();
                ASTNode right = ParseComparison();
                conditions.Add(new LogicalNode(left, logicalOp, right));
                left = right;
            }

            return conditions;
        }

        private ASTNode ParseComparison()
        {
            ASTNode left = ParseExpression();

            while (Peek().Type == TokenType.Greater || Peek().Type == TokenType.Less ||
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
            Consume(TokenType.LeftParen, "Expected '(' after 'input' ");
            Token id = Consume(TokenType.Identifier, "Expected  variable name");
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
            if (Peek().Type == TokenType.LogicalAnd)
            {
                return null;
            }
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
                case TokenType.LogicalAnd:
                    _position++;
                    return null;

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

