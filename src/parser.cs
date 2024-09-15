
using System.Collections;
using System.Runtime.InteropServices;

namespace VSharp
{
   
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

            while (Peek().Type != TokenType.EndOfInput)
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
                case TokenType.KeywordWhile:
                    return ParseWhileStatement();
                case TokenType.KeywordFunc:
                    return ParseFuncStatement();
                
                default:
                    //allow for expressions to be statements in this case:
                    //if statements and function calls are expresions
                    return new ExprStatement(ParseExpression());
            }
        }



        private Expression ParseArray() 
        {
            Consume(TokenType.SquareOpen, "");
            if (Peek().Type == TokenType.SquareClose) 
            {
                Consume(TokenType.SquareClose, "Expeced `]`");
                return new ConstArray();
            }

            if (Peek2().Type == TokenType.Assignment)
            {
                return ParseObject();
            }

            List<Expression> expressions = new List<Expression>(); 

            while (true) 
            {
                expressions.Add(ParseExpression());
                switch (NextToken().Type) 
                {
                    case TokenType.Comma:
                        continue;
                    case TokenType.SquareClose:
                        return new ConstArray(expressions);
                    default:
                        throw new Exception("Expected `,` or `]`");
                }
            }
        }

        private ConstObject ParseObject() 
        {
            Dictionary<string, Expression> entries = new Dictionary<string, Expression>();
            while (true)
            {
                string name = Consume(TokenType.Identifier, "Expected identifier").Value;
                Consume(TokenType.Assignment, "Expected =");
                Expression value = ParseExpression();
                entries[name] = value;
                switch (NextToken().Type)
                {
                    case TokenType.Comma:
                        continue;
                    case TokenType.SquareClose:
                        return new ConstObject { Entries = entries };
                    default:
                        throw new Exception("Expected , or ]");
                }
            }
        }


        private ASTNode ParseFuncStatement()
        {
            Consume(TokenType.KeywordFunc, "Expected 'func' keyword");
            Expression identifier = ParseExpression();
            BlockNode block = ParseBlockNode();

            return identifier switch {
                Invokation inv => new FuncStatementNode
                {
                    Args = inv.Args.Select(it => (it as IdentifierNode)?.Name ?? throw new Exception("Failed")).ToList(),
                    Block = block,
                    Name = (inv.Parent as IdentifierNode)?.Name ?? throw new Exception("Cannot defined method")
                },
                MethodCall pa => new PropertyAssignment {
                    Parent = pa.Parent,
                    Name = pa.Name,
                    Value = new ConstFunction {
                        Args = pa.Args.Select(it => (it as IdentifierNode)?.Name ?? throw new Exception("Failed")).ToList(),
                        Body = block
                    }
                },
                _ => throw new Exception("Cannot assign to provided expression")
            };
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


        private WhileStatementNode ParseWhileStatement()
        {
            Consume(TokenType.KeywordWhile, "Expected 'while' keyword");
            Consume(TokenType.LeftParen, "Expected '(' after 'while'");
            var condition = ParseLogicalExpression();
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
            var condition = ParseLogicalExpression();
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


        private Expression ParseLogicalExpression()
        {
            Expression node = ParseComparison();

            while (Peek().Type == TokenType.LogicalOr || Peek().Type == TokenType.LogicalAnd)
            {
                Token logicalOp = NextToken();
                Expression right = ParseComparison();
                node = new LogicalNode(node, logicalOp, right);
            }

            return node;
        }

        private Expression ParseComparison()
        {
            Expression node = ParseExpression();

            if (Peek().Type == TokenType.Greater || Peek().Type == TokenType.Less ||
                Peek().Type == TokenType.Equal || Peek().Type == TokenType.NotEqual ||
                Peek().Type == TokenType.GreaterEqual || Peek().Type == TokenType.LessEqual)
            {
                Token comparisonOp = NextToken();
                Expression right = ParseExpression();
                node = new BinaryOperationNode(node, comparisonOp.Value, right);
            }

            return node;
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

        private ASTNode ParseSetStatement()
        {
            Consume(TokenType.KeywordSet, "Expected 'set' keyword.");
            Expression assignee = ParseExpression();
            Consume(TokenType.Assignment, "Expected '=' after variable name.");
            Expression expression = ParseExpression();
            return assignee switch
            {
                IdentifierNode identifier => new SetStatementNode(identifier.Name, expression),
                PropertyAccess pa => new PropertyAssignment { Name = pa.Name, Parent = pa.Parent, Value = expression },
                _ => throw new Exception("Invalid syntax cannot set expr on the left side"),
            };
        }


        private Expression ParseExpression()
        {
            return ParseTerm();
        }

        private Expression ParseTerm()
        {
            Expression node = ParseFactor();

            if (Peek().Type == TokenType.Operator && (Peek().Value == "+" || Peek().Value == "-"))
            {
                Token operatorToken = NextToken();
                Expression right = ParseTerm();
                node = new BinaryOperationNode(node, operatorToken.Value, right);
            }

            return node;
        }

        private Expression ParseFactor()
        {
            Expression node = ParseCall();

            if (Peek().Type == TokenType.Operator && (Peek().Value == "*" || Peek().Value == "/"))
            {
                Token operatorToken = NextToken();
                Expression right = ParseFactor();
                node = new BinaryOperationNode(node, operatorToken.Value, right);
            }

            return node;
        }

        private Expression ParseCall()
        {
            Expression node = ParsePrimary();


            while (Peek().Type == TokenType.LeftParen || Peek().Type == TokenType.Dot)
            {
                Token next = Peek();
                if (next.Type == TokenType.Dot)
                {
                    NextToken();
                    string name = Consume(TokenType.Identifier, "").Value;
                    node = new PropertyAccess { Parent = node, Name = name};
                } 

                if (next.Type == TokenType.LeftParen)
                {
                    List<Expression> args = ParseCallingArgs();
                    if (node is PropertyAccess n)
                    {
                        node = new MethodCall { Args = args, Name = n.Name, Parent = n.Parent };
                    } else 
                    {
                        node = new Invokation { Args = args, Parent = node };
                    }
                }
               
            }

            return node;
        }

        private List<Expression> ParseCallingArgs()
        {
            Consume(TokenType.LeftParen, "Expected (");

            if (Peek().Type == TokenType.RightParen)
            {
                NextToken();
                return new List<Expression>();
            }

            List<Expression> arguments = new List<Expression>();
            while (true)
            {
                arguments.Add(ParseExpression());
                Token next = NextToken();
                switch (next.Type) 
                {
                    case TokenType.Comma:
                        continue;
                    case TokenType.RightParen:
                        return arguments;
                    default:
                        throw new Exception("Expected , or )");
                }
            }
        }


        private Expression ParsePrimary()
        {
            Token current = Peek();

            switch (current.Type)
            {
                case TokenType.IntegerLiteral:
                    NextToken();
                    return new ConstInt { Value = int.Parse(current.Value)};
                case TokenType.FloatLiteral:
                    NextToken();
                    return new ConstDouble { Value = double.Parse(current.Value) };
                case TokenType.StringLiteral:
                    NextToken();
                    return new ConstString { Value = current.Value };
                case TokenType.SquareOpen:
                    return ParseArray();
                case TokenType.Identifier:
                    NextToken();
                    return new IdentifierNode(current.Value);
                case TokenType.KeywordIf:
                    return ParseIfStatement();
                case TokenType.KeywordFunc:
                    return ParseAnonymousFunc();
                case TokenType.LeftParen:
                    NextToken();
                    Expression expr = ParseExpression();
                    Consume(TokenType.RightParen, "Expected closing brace");
                    return expr;
                default:
                    throw new Exception($"Unexpected token: {current}");
            }
        }

        private ConstFunction ParseAnonymousFunc() 
        {
            Consume(TokenType.KeywordFunc, "Expected func keyword");
            ArgNode args = ParseArgs();
            Expression body = ParseBlockNode();

            return new ConstFunction { Args = args.Names, Body = body };
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

        private Token Peek2()
        {
            if (_position + 1 >= _tokens.Count)
            {
                return new Token(TokenType.EndOfInput, "");
            }
            return _tokens[_position + 1];
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
