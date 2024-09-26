using System.Collections.Generic;

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

    public class ExprStatement : ASTNode
    {
        public Expression Expression { get; }

        public ExprStatement(Expression expr)
        {
            Expression = expr;
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
    public class FuncStatementNode : ASTNode
    {
        public string Name { get; set; }
        public List<string> Args { get; set; }
        public BlockNode Block { get; set; }
        public Dictionary<string, object> Vars { get; set; }

        public FuncStatementNode()
        {
            Block = new BlockNode();
        }
    }


    public class WhileStatementNode : ASTNode
    {
        public Expression Condition { get; set; }
        public Expression TrueBlock { get; set; }

        public WhileStatementNode()
        {
            TrueBlock = new BlockNode();
        }
    }

    public class ForLoop : ASTNode
    {
        public required Expression Body;
        public required Expression Parent;
        public required string ItemName;
    }

    public class SetStatementNode : ASTNode
    {
        public string VariableName { get; }
        public Expression Expression { get; }

        public SetStatementNode(string variableName, Expression expression)
        {
            VariableName = variableName;
            Expression = expression;
        }
    }

    public class ImportStatemnt : ASTNode
    {
        public Expression Expression { get; }
        public ImportStatemnt(Expression expr)
        {
            this.Expression = expr;
        }
    }

    public class PropertyAssignment : ASTNode
    {
        public required Expression Parent;
        public required string Name;
        public required Expression Value;
    }

    public class IndexAssignment : ASTNode
    {
        public required Expression Parent;
        public required Expression Index;
        public required Expression Value;
    }

    public abstract class Expression { }

    public class ConstString : Expression
    {
        public required string Value { get; set; }
    }

    public class ConstInt : Expression
    {
        public required int Value { get; set; }
    }

    public class ConstDouble : Expression
    {
        public required double Value { get; set; }
    }


    public class IdentifierNode : Expression
    {
        public string Name { get; }

        public IdentifierNode(string name)
        {
            Name = name;
        }
    }

    public class ConstArray : Expression
    {
        public List<Expression> Expressions;

        public ConstArray()
        {
            Expressions = new List<Expression>();
        }

        public ConstArray(List<Expression> expresions)
        {
            Expressions = expresions;
        }
    }

    public class ConstObject : Expression
    {
        public required Dictionary<string, Expression> Entries { get; set; }

    }

    public class ConstFunction : Expression
    {
        public required List<string> Args;
        public required Expression Body;
    }


    public class BinaryOperationNode : Expression
    {
        public Expression Left { get; }
        public string Operator { get; }
        public Expression Right { get; }

        public BinaryOperationNode(Expression left, string operatorSymbol, Expression right)
        {
            Left = left;
            Operator = operatorSymbol;
            Right = right;
        }
    }

    public class PropertyAccess : Expression
    {
        public required Expression Parent;
        public required string Name;
    }


    public class MethodCall : Expression
    {
        public required Expression Parent { get; set; }
        public required string Name { get; set; }
        public required List<Expression> Args { get; set; }
    }

    public class LogicalNode : Expression
    {
        public Expression Left { get; set; }
        public Token Operator { get; set; }
        public Expression Right { get; set; }

        public LogicalNode(Expression left, Token operatorSymbol, Expression right)
        {
            Left = left;
            Operator = operatorSymbol;
            Right = right;
        }
    }


    public class Invokation : Expression
    {
        public required List<Expression> Args { get; set; }
        public required Expression Parent { get; set; }

    }


    public class IfNode : Expression
    {
        public Expression Condition { get; set; }
        public Expression TrueBlock { get; set; }
        public Expression FalseBlock { get; set; }

        public IfNode()
        {
            TrueBlock = new BlockNode();
            FalseBlock = new BlockNode();
        }
    }

    public class Indexing : Expression
    {
        public required Expression Parent;
        public required Expression Index;
    }

    public class BlockNode : Expression
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
}