namespace VSharp
{
   
    public class Parser(List<Token> tokens)
    {
        private readonly List<Token> _tokens = tokens;
        private int _position = 0;

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

            return current.Type switch
            {
                TokenType.KeywordSet => ParseSetStatement(),
                TokenType.Identifier => ParseAssignmentOrExpression(),
                TokenType.KeywordWhile => ParseWhileStatement(),
                TokenType.KeywordFunc => ParseFuncStatement(),
                TokenType.KeywordFor => ParseForLoop(),
                TokenType.KeywordReturn => ParseReturn(),
                TokenType.KeywordBreak => ParseBreak(),
                TokenType.KeywordContinue => ParseContinue(),
                TokenType.KeywordType => ParseTypeStatement(),
                TokenType.KeywordImport => ParseImport(),
                TokenType.KeywordLib => ParseLib(),
                _ => new ExprStatement(ParseExpression()),
            };
        }

        private ASTNode ParseAssignmentOrExpression()
        {
            int startPosition = _position;
            Expression leftSide = ParseExpression();

            if (Peek().Type == TokenType.Assignment)
            {
                
                _position = startPosition;
                return ParseSetStatement(false);
            }
            else
            {
              
                return new ExprStatement(leftSide);
            }
        }

        private LibStatement ParseLib()
        {
            Consume(TokenType.KeywordLib, "Expected 'lib' keyword.");
            var path = ParseExpression();
            string? name = null;
            Consume(TokenType.KeywordAs, "Expected 'as' keyword.");
  
 
             name = Consume(TokenType.Identifier, "Expected identifer").Value;
            

            return new LibStatement(path, name);
        }

        private ImportStatement ParseImport()
        {
            Consume(TokenType.KeywordImport, "Expected 'import' keyword.");
            var path = ParseExpression();
            string? name = null;
            if (Peek().Type == TokenType.KeywordAs)
            {
                NextToken();
                name = Consume(TokenType.Identifier, "Expected identifer").Value;
            }

            return new ImportStatement(path, name);
        }

        private TypeStatement ParseTypeStatement()
        {
            Consume(TokenType.KeywordType, "Expected the type keyword");
            string name = Consume(TokenType.Identifier, "Expected type name").Value;
            string[] generics = [];
            if (Peek().Type == TokenType.Less)
            {
                generics = ParseGenerics();
            }
            Consume(TokenType.Assignment, "Exepected `=` operator");
            VType type = ParseType();

            return new TypeStatement {
                Generics = generics,
                Name = name,
                Type = type
            };
        }

        private Return ParseReturn()
        {
            Consume(TokenType.KeywordReturn, "Expected return keyowrd");
            if (Peek().Type == TokenType.RightBrace || Peek().Type == TokenType.EndOfInput)
            {
                return new Return { Expr = null };
            }  else {
                return new Return { Expr = ParseExpression() };
            }
        }
        
        private Break ParseBreak()
        {
            Consume(TokenType.KeywordBreak, "Expected break keyowrd");
            if (Peek().Type == TokenType.RightBrace || Peek().Type == TokenType.EndOfInput)
            {
                return new Break { Expr = null };
            }  else {
                return new Break { Expr = ParseExpression() };
            }
        }

        private Continue ParseContinue()
        {
            Consume(TokenType.KeywordContinue, "Expected continue keyword");
            return new Continue();
        }

        private ForLoop ParseForLoop()
        {
            Consume(TokenType.KeywordFor, "Expected keyword for");
            Consume(TokenType.LeftParen, "Expected ( after for");
            string name = Consume(TokenType.Identifier, "Expected itemname in for loop").Value;
            Consume(TokenType.KeywordIn, "Expected `in`");
            Expression parent = ParseExpression();
            Consume(TokenType.RightParen, "Expected ) after for");
            Expression body = ParseBlockNode();
            return new ForLoop { Body = body, ItemName = name, Parent = parent };
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

            List<Expression> expressions = new(); 

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
            Dictionary<object, Expression> entries = [];
            while (true)
            {
                object key = ParseExpression() switch {
                    IdentifierNode n => n.Name,
                    ConstString s => s.Value,
                    ConstInt i => i.Value,
                    _ => throw new Exception("Invalid key")
                };
                Consume(TokenType.Assignment, "Expected =");
                Expression value = ParseExpression();
                entries[key] = value;
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

            string[] generics = [];
            if (Peek().Type == TokenType.Less)
            {
                generics = ParseGenerics();
            }

            Expression identifier = ParseCall(false); //disallow invoke expressions to be art of this
            var args = ParseArgs();

            VType? returnType = null;
            if (Peek().Type == TokenType.Colon) 
            {
                NextToken();
                returnType = ParseType();
            }
            
            BlockNode block = ParseBlockNode();
            ConstFunction func = new()
            {
                Args = args,
                Body = block,
                ReturnType = returnType,
                Generics = generics
            };

            return identifier switch
            {
                IdentifierNode i => new SetStatementNode
                (
                    i.Name,
                    func
                ),
                Indexing idx => new IndexAssignment
                {
                    Index = idx.Index,
                    Parent = idx.Parent,
                    Value = func
                },
                MethodCall pa => new PropertyAssignment
                {
                    Parent = pa.Parent,
                    Name = pa.Name,
                    Value = func
                },
                _ => throw new Exception("Cannot assign to provided expression"),
            };
        }

        private string[] ParseGenerics()
        {
            Consume(TokenType.Less, "Expected generic definitions");
            if (Peek().Type == TokenType.Greater)
            {
                return [];
            }

            List<string> generic_names = [];
            while(true)
            {
                generic_names.Add(Consume(TokenType.Identifier, "Expected generic name").Value);
                Token next = NextToken();
                switch (next.Type)
                {
                case TokenType.Comma:
                    continue;
                case TokenType.Greater:
                    return generic_names.ToArray();
                default:
                    throw new Exception("Expected , or >");
                }
            }
        }

        private List<(string, VType?)> ParseArgs()
        {
            List<(string, VType?)> args = [];
            Consume(TokenType.LeftParen, "Expected '(' after 'func'");

            if (Peek().Type== TokenType.RightParen)
            {
                NextToken();
                return args;
            }

            while (true)
            {
                string name = Consume(TokenType.Identifier, "Expected argument name").Value;
                VType? type = null;
                if (Peek().Type == TokenType.Colon) 
                {
                    NextToken();
                    type = ParseType();
                }
                args.Add((name, type));

                Token next = NextToken();
                switch (next.Type) {
                case TokenType.Comma:
                    continue;
                case TokenType.RightParen:
                    return args;
                default:
                    throw new Exception("Unexpected token");
                }
            }
        }

        private	VType ParseType()
        {
            Token next = NextToken();
            VType type;
            switch (next.Type) 
            {
            case TokenType.SquareOpen:
                type = ParseArrayOrObjectType();
                break;
            case TokenType.KeywordFunc:
                type = ParseFunctionType();
                break;
            case TokenType.LeftParen:
                type = ParseType();
                Consume(TokenType.RightParen, "Expected )");
                break;
            case TokenType.Identifier:
                List<string> identifiers = [next.Value];
                while(Peek().Type == TokenType.Dot) 
                {
                    NextToken();
                    identifiers.Add(Consume(TokenType.Identifier, "Expected identifier").Value);                    
                }
                VType[] generics = [];
                if (Peek().Type == TokenType.Less)
                {
                    generics = ParseTypeArguments();
                }

                type = new VType.Normal(identifiers.ToArray(), generics);
                break;
            default:
                throw new Exception($"Invalid character while parsing a type {next}");
            }
            
            if (Peek().Type == TokenType.Or)
            {
                NextToken();
                return type.Join(ParseType());
            }
            if (Peek().Type == TokenType.And)
            {
                NextToken();
                return type.Intersect(ParseType());
            }
            return type;
        }

        private VType ParseFunctionType()
        {
            VType[] types = ParseTypeFuntionArgs();
            Consume(TokenType.Colon, "Expected return type");
            VType returnType = ParseType();

            return new VType.Func(types, returnType);
        }

        private VType[] ParseTypeFuntionArgs()
        {
            Consume(TokenType.LeftParen, "Expected (");

            if (Peek().Type == TokenType.RightParen)
            {
                NextToken();
                return Array.Empty<VType>();
            } 
            List<VType> types = new();

            while(true)
            {
                types.Add(ParseType());
                Token next = NextToken();
                switch (next.Type)
                {
                case TokenType.Comma:
                    continue;
                case TokenType.RightParen:
                    return types.ToArray();
                default:
                    throw new Exception("Expected , or )");
                }
            }
        }

        private VType[] ParseTypeArguments()
        {
            Consume(TokenType.Less, "Expected type arguments");
            List<VType> types = new();

            while(true) 
            {
                types.Add(ParseType());
                Token next = NextToken();
                switch (next.Type){
                case TokenType.Comma:
                    continue;
                case TokenType.Greater:
                    return types.ToArray();
                default: 
                    throw new Exception("Expected , or >");
                }
            }
        }

        private VType ParseArrayOrObjectType()
        {
            if (Peek().Type == TokenType.SquareClose)
            {
                return new VType.Object([]);
            }
            if (Peek2().Type == TokenType.Colon) 
            {
                //object
                Dictionary<string, VType> entries = [];
            
                while(true) {
                    string name = Consume(TokenType.Identifier, "Expected identifier").Value;
                    Consume(TokenType.Colon, "Expected colon in type defintion");
                    var type = ParseType();
                    entries[name] = type;
                
                    Token next = NextToken();
                    switch (next.Type) 
                    {
                    case TokenType.SquareClose:
                        return new VType.Object(entries);
                    case TokenType.Comma:
                        continue;
                    default:
                        throw new Exception($"Expected ] or , and got {next.Value}");
                    }
                }
            } else {
                //array
                VType itemType = ParseType();
                Consume(TokenType.SquareClose, "");
                return new VType.Array(itemType);
            }
           
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
            List<ASTNode> statements = [];

            Consume(TokenType.LeftBrace, "Expected '{'");

            while (!IsAtEnd() && Peek().Type != TokenType.RightBrace)
            {
                statements.Add(ParseStatement());
            }

            Consume(TokenType.RightBrace, "Expected '}'");

            return new BlockNode(statements);
        }

        private ASTNode ParseSetStatement(bool con = true)
        {
            if (con)
            {
                Consume(TokenType.KeywordSet, "Expected 'set' keyword.");
            }
            
            Expression assignee = ParseExpression();
            Consume(TokenType.Assignment, "Expected '=' after variable name.");
            Expression expression = ParseExpression();
            return assignee switch
            {
                IdentifierNode identifier => new SetStatementNode(identifier.Name, expression),
                PropertyAccess pa => new PropertyAssignment { Name = pa.Name, Parent = pa.Parent, Value = expression },
                Indexing indexing => new IndexAssignment { Index = indexing.Index, Parent = indexing.Parent, Value = expression },
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

        private Expression ParseCall(bool allowCalls = true)
        {
            Expression node = ParsePrimary();


            while ((Peek().Type == TokenType.LeftParen && allowCalls) || Peek().Type == TokenType.Dot || Peek().Type == TokenType.SquareOpen || Peek().Type == TokenType.KeywordIn || Peek().Type == TokenType.KeywordIs)
            {
                Token next = Peek();
                if (next.Type == TokenType.Dot)
                {
                    NextToken();
                    string name = Consume(TokenType.Identifier, "").Value;
                    node = new PropertyAccess { Parent = node, Name = name};
                } 

                if (next.Type == TokenType.KeywordIn)
                {
                    NextToken();
                    Expression parent = ParseExpression();
                    node = new HasElementCheck { Item = node, Container = parent };
                }

                if (next.Type == TokenType.KeywordIs)
                {
                    NextToken();
                    VType type = ParseType();
                    node = new TypeCheck { Item = node, Type = type };
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

                if (next.Type == TokenType.SquareOpen)
                {
                    NextToken();
                    Expression index = ParseExpression();
                    Consume(TokenType.SquareClose, "Expected `]`");
                    node = new Indexing { Parent = node, Index = index };
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
                return [];
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
                        throw new Exception($"Expected , or ) but got {next}");
                }
            }
        }


        private Expression ParsePrimary()
        {
            Token current = Peek();

            switch (current.Type)
            {
                case TokenType.KeywordTrue:
                    NextToken();
                    return new ConstBool { Value = true };
                case TokenType.KeywordFalse:
                    NextToken();
                    return new ConstBool { Value = false };
                case TokenType.IntegerLiteral:
                    NextToken();
                    return new ConstInt { Value = int.Parse(current.Value)};
                case TokenType.FloatLiteral:
                    NextToken();
                    return new ConstDouble { Value = double.Parse(current.Value.Replace('.', ',')) };
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
                case TokenType.ExclamationMark:
                    NextToken();
                    return new Not { Value = ParseCall() };
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

            string[] generics = Array.Empty<string>();
            if (Peek().Type == TokenType.Less)
            {
                generics = ParseGenerics();
            }

            var args = ParseArgs();
            VType? returnType = null;
            if (Peek().Type == TokenType.Colon) 
            {
                NextToken();
                returnType = ParseType();
            }
            Expression body = ParseBlockNode();

            return new ConstFunction { Args = args, Body = body, ReturnType = returnType, Generics = generics };
        }

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Peek().Type == type)
            {
                return NextToken();
            }

            throw new Exception(errorMessage + " got "+Peek().Type);
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
